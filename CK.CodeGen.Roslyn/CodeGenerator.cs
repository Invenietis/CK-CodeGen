using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Text;
using CK.CodeGen;
using CK.Core;

namespace CK.CodeGen
{
    /// <summary>
    /// Encapsulates Roslyn compiler.
    /// </summary>
    public class CodeGenerator
    {
        readonly Func<ICodeWorkspace> _workspaceFactory;

        /// <summary>
        /// Initializes a new <see cref="CodeGenerator"/> with options.
        /// </summary>
        /// <param name="workspaceFactory">
        /// Factory for <see cref="ICodeWorkspace"/> implementations.
        /// Must not be null.
        /// </param>
        public CodeGenerator( Func<ICodeWorkspace> workspaceFactory )
        {
            if( workspaceFactory == null ) throw new ArgumentNullException( nameof( workspaceFactory ) );
            _workspaceFactory = workspaceFactory;
            CompilationOptions = new CSharpCompilationOptions( OutputKind.DynamicallyLinkedLibrary );
        }

        /// <summary>
        /// Gets or sets the parse options to use.
        /// Default to null: all default applies, the language version is <see cref="LanguageVersion.Default"/>.
        /// </summary>
        public CSharpParseOptions ParseOptions { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="CSharpCompilationOptions"/>.
        /// Defaults to the option default initialized to produce <see cref="OutputKind.DynamicallyLinkedLibrary"/> output.
        /// </summary>
        public CSharpCompilationOptions CompilationOptions { get; set; }

        /// <summary>
        /// Gets or sets whether the assembly that defines the object type is
        /// automaticaaly registered.
        /// Defaults to true.
        /// </summary>
        public bool AutoRegisterRuntimeAssembly { get; set; } = true;

        /// <summary>
        /// Gets a mutable list of <see cref="ICodeGeneratorModule"/>.
        /// Since a code module can maitain an internal state between the calls to <see cref="ICodeGeneratorModule.Rewrite(IReadOnlyList{SyntaxTree})"/>
        /// and <see cref="ICodeGeneratorModule.Inject(ICodeWorkspace)"/>, this list is cleared by each
        /// call to Generate instance methods.
        /// </summary>
        public List<ICodeGeneratorModule> Modules { get; } = new List<ICodeGeneratorModule>();

        /// <summary>
        /// Generates an assembly from a source, a minimal list of required reference assemblies.
        /// </summary>
        /// <param name="sourceCode">The source code. Must be valid C# code.</param>
        /// <param name="assemblyPath">The full final assembly path (including the .dll extension). Can be null if skipCompilation is true.</param>
        /// <param name="someReferences">List of reference assemblies that can be a subset of the actual dependencies.</param>
        /// <param name="skipCompilation">True to skip the compilation. Only the parsing and the source generation is done.</param>
        /// <param name="loader">Optional loader function to load the final emitted assembly.</param>
        /// <returns>Encapsulation of the result.</returns>
        public GenerateResult Generate( string sourceCode, string assemblyPath, IEnumerable<Assembly> someReferences, bool skipCompilation, Func<string, Assembly> loader = null )
        {
            var w = _workspaceFactory();
            if( !String.IsNullOrWhiteSpace( sourceCode ) ) w.Global.Append( sourceCode );
            foreach( var a in someReferences ) w.DoEnsureAssemblyReference( a );
            return Generate( w, assemblyPath, skipCompilation, loader );
        }

        /// <summary>
        /// Generates an assembly from a source and a minimal list of required reference assemblies.
        /// </summary>
        /// <param name="code">The source code.</param>
        /// <param name="assemblyPath">The full final assembly path (including the .dll extension). Can be null if skipCompilation is true.</param>
        /// <param name="skipCompilation">True to skip the compilation. Only the parsing and the source generation is done.</param>
        /// <param name="loader">Optional loader function to load the final emitted assembly.</param>
        /// <returns>Encapsulation of the result.</returns>
        public GenerateResult Generate( ICodeWorkspace code, string assemblyPath, bool skipCompilation, Func<string, Assembly> loader = null )
        {
            if( code == null ) throw new ArgumentNullException( nameof( code ) );
            using( var weakLoader = WeakAssemblyNameResolver.TemporaryInstall() )
            {
                var input = GeneratorInput.Create( _workspaceFactory, code, Modules, !skipCompilation && AutoRegisterRuntimeAssembly, ParseOptions );
                Modules.Clear();

                if( skipCompilation ) return new GenerateResult( input.Trees );

                var collector = new HashSet<Assembly>();
                foreach( var a in input.Assemblies )
                {
                    if( collector.Add( a ) ) Discover( a, collector );
                }
                return Generate( CompilationOptions,
                                 input.Trees,
                                 assemblyPath,
                                 collector.Select( a => MetadataReference.CreateFromFile( new Uri( a.CodeBase ).LocalPath ) ),
                                 loader )
                        .WithLoadFailures( weakLoader.Conflicts );
            }
        }

        static void Discover( Assembly a, HashSet<Assembly> collector )
        {
            foreach( var name in a.GetReferencedAssemblies() )
            {
                var dep = Assembly.Load( name );
                if( collector.Add( dep ) ) Discover( dep, collector );
            }
        }

        /// <summary>
        /// Generates an assembly from a <see cref="SyntaxTree"/> list and a
        /// list of <see cref="MetadataReference"/> required reference assemblies.
        /// <para>
        /// Caution: this method is not protected by the <see cref="WeakAssemblyNameResolver"/>.
        /// It should be done, if necessary, by the caller.
        /// </para>
        /// </summary>
        /// <param name="compileOptions">Compilation options.</param>
        /// <param name="trees">The syntax trees.</param>
        /// <param name="assemblyPath">The full final assembly path (including the .dll extension).</param>
        /// <param name="allReferences">List of assemblies' references.</param>
        /// <param name="loader">Optional loader function to load the final emitted assembly.</param>
        /// <returns>Encapsulation of the result.</returns>
        static public GenerateResult Generate(
            CSharpCompilationOptions compileOptions,
            IReadOnlyList<SyntaxTree> trees,
            string assemblyPath,
            IEnumerable<MetadataReference> allReferences,
            Func<string, Assembly> loader = null )
        {
            if( assemblyPath == null ) throw new ArgumentNullException( nameof( assemblyPath ) );
            try
            {
                var option = compileOptions.WithAssemblyIdentityComparer( DesktopAssemblyIdentityComparer.Default );
                CSharpCompilation compilation = CSharpCompilation.Create(
                    Path.GetFileNameWithoutExtension( assemblyPath ),
                    trees,
                    allReferences,
                    option );

                var r = compilation.Emit( assemblyPath );
                if( r.Success && loader != null )
                {
                    try
                    {
                        return new GenerateResult( null, trees, r, loader( assemblyPath ), null, null );
                    }
                    catch( Exception ex )
                    {
                        return new GenerateResult( null, trees, r, null, ex, null );
                    }
                }
                return new GenerateResult( null, trees, r, null, null, null );
            }
            catch( Exception ex )
            {
                return new GenerateResult( ex, null, null, null, null, null );
            }
        }
    }
}
