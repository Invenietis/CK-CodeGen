using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Text;
using CK.CodeGen.Abstractions;

namespace CK.CodeGen
{
    /// <summary>
    /// Encapsulates Roslyn compiler.
    /// </summary>
    public class CodeGenerator
    {
        readonly CSharpCompilationOptions _options;

        /// <summary>
        /// Initializes a new <see cref="CodeGenerator"/> with defult options.
        /// </summary>
        public CodeGenerator()
            : this( null )
        {
        }

        /// <summary>
        /// Initializes a new <see cref="CodeGenerator"/> with options.
        /// </summary>
        /// <param name="options">Compilation options.</param>
        public CodeGenerator( CSharpCompilationOptions options )
        {
            if( options == null ) options = new CSharpCompilationOptions( OutputKind.DynamicallyLinkedLibrary );
            _options = options;
        }

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
        /// Generates an assembly from a source, a minimal list of required reference assemblies and, with the help 
        /// of the <paramref name="resolver"/>, computes all required dependencies.
        /// </summary>
        /// <param name="sourceCode">The source code. Must be valid C# code.</param>
        /// <param name="assemblyPath">The full final assembly path (including the .dll extension).</param>
        /// <param name="someReferences">List of reference assemblies that can be a subset of the actual dependencies.</param>
        /// <param name="resolver">Must load an assembly from its name.</param>
        /// <param name="loader">Optional loader function to load the final emitted assembly.</param>
        /// <returns>Encapsulation of the result.</returns>
        public GenerateResult Generate( string sourceCode, string assemblyPath, IEnumerable<Assembly> someReferences, IAssemblyResolver resolver, Func<string, Assembly> loader = null )
        {
            return Generate( CodeWorkspace.Create( sourceCode, someReferences.ToArray() ), assemblyPath, resolver, loader );
        }

        /// <summary>
        /// Generates an assembly from a source, a minimal list of required reference assemblies and, with the help 
        /// of the <paramref name="resolver"/>, computes all required dependencies.
        /// </summary>
        /// <param name="code">The source code.</param>
        /// <param name="assemblyPath">The full final assembly path (including the .dll extension).</param>
        /// <param name="resolver">Must load an assembly from its name.</param>
        /// <param name="loader">Optional loader function to load the final emitted assembly.</param>
        /// <returns>Encapsulation of the result.</returns>
        public GenerateResult Generate( ICodeWorkspace code, string assemblyPath, IAssemblyResolver resolver, Func<string, Assembly> loader = null )
        {
            if( code == null ) throw new ArgumentNullException( nameof( code ) );
            if( resolver == null ) throw new ArgumentNullException( nameof( resolver ) );
            using( CK.Core.WeakAssemblyNameResolver.TempInstall() )
            {
                var input = GeneratorInput.Create( code, Modules, AutoRegisterRuntimeAssembly );
                Modules.Clear();
                var closureResult = resolver.GetAssembliesClosure( input.Assemblies );
                return Generate(
                        _options,
                        input.Trees,
                        assemblyPath,
                        closureResult.AllAssemblies.Select( a => MetadataReference.CreateFromFile( resolver.GetAssemblyFilePath( a ) ) ),
                        loader ).WithLoadFailures( closureResult.LoadFailures );
            }
        }

        /// <summary>
        /// Generates an assembly from a <see cref="SyntaxTree"/> list and a
        /// list of <see cref="MetadataReference"/> required reference assemblies.
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
            using( CK.Core.WeakAssemblyNameResolver.TempInstall() )
            {
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
}
