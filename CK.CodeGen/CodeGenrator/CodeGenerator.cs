using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Text;

namespace CK.CodeGen
{
    public class CodeGenerator
    {
        readonly CSharpCompilationOptions _options;

        static CodeGenerator()
        {
        }

        public CodeGenerator()
            : this(null)
        {
        }

        public List<ICodeGeneratorModule> Modules { get; } = new List<ICodeGeneratorModule>();


        public CodeGenerator(CSharpCompilationOptions options)
        {
            if (options == null) options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            _options = options;
        }

        /// <summary>
        /// Generates an assembly from a source, a minimal list of required reference assemblies and, with the help 
        /// of the <paramref name="resolver"/>, computes all required dependencies.
        /// </summary>
        /// <param name="sourceCode">The source code. Must be vali C# code.</param>
        /// <param name="assemblyPath">The full final assemblt path (including the .dll extension).</param>
        /// <param name="someReferences">List of reference assemblies that can be a subset of the actual dependencies.</param>
        /// <param name="resolver">Must load an assembly from its name.</param>
        /// <param name="loader">Optional loader function to load the final emitted assembly.</param>
        /// <returns>Encapsulation of the result.</returns>
        public GenerateResult Generate(string sourceCode, string assemblyPath, IEnumerable<Assembly> someReferences, IAssemblyResolver resolver, Func<string, Assembly> loader = null)
        {
            if (someReferences == null) throw new ArgumentNullException(nameof(someReferences));
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));
#if NET461
            using( CK.Core.WeakAssemblyNameResolver.TempInstall())
            {
#endif
            var fromModules = Modules.SelectMany( m => m.RequiredAssemblies );
            var closureResult = resolver.GetAssembliesClosure( someReferences.Concat( fromModules ) );
            return Generate(
                    sourceCode, 
                    assemblyPath, 
                    closureResult.AllAssemblies.Select(a => MetadataReference.CreateFromFile(resolver.GetAssemblyFilePath(a))), 
                    loader).WithLoadFailures( closureResult.LoadFailures );
#if NET461
            }
#endif
        }

        /// <summary>
        /// Generates an assembly from a source code and a list of absolute path to required reference assemblies.
        /// </summary>
        /// <param name="sourceCode">The source code. Must be valid C# code.</param>
        /// <param name="assemblyPath">The full final assemblt path (including the .dll extension).</param>
        /// <param name="allRefAssemblyPaths">List of full paths to reference assemblies.</param>
        /// <param name="loader">Optional loader function to load the final emitted assembly.</param>
        /// <returns>Encapsulation of the result.</returns>
        public GenerateResult Generate( string sourceCode, string assemblyPath, IEnumerable<string> allRefAssemblyPaths, Func<string, Assembly> loader = null )
        {
#if NET461
            using( CK.Core.WeakAssemblyNameResolver.TempInstall())
            {
#endif
            try
            {
                return Generate( sourceCode, assemblyPath, allRefAssemblyPaths.Select( p => MetadataReference.CreateFromFile( p ) ), loader );
            }
            catch( Exception ex )
            {
                return new GenerateResult(ex, null, null, null, null, null);
            }
#if NET461
            }
#endif
        }

        /// <summary>
        /// Generates an assembly from a source code and a list of Roselyn <see cref="MetadataReference"/> required reference assemblies.
        /// </summary>
        /// <param name="sourceCode">The source code. Must be vali C# code.</param>
        /// <param name="assemblyPath">The full final assemblt path (including the .dll extension).</param>
        /// <param name="allReferences">List of assemblies' references.</param>
        /// <param name="loader">Optional loader function to load the final emitted assembly.</param>
        /// <returns>Encapsulation of the result.</returns>
        public GenerateResult Generate(
            string sourceCode, 
            string assemblyPath, 
            IEnumerable<MetadataReference> allReferences, 
            Func<string, Assembly> loader = null)
        {
#if NET461
            using( CK.Core.WeakAssemblyNameResolver.TempInstall())
            {
#endif
            try
            {
                SyntaxTree tree = SyntaxFactory.ParseSyntaxTree( sourceCode );
                var trees = new List<SyntaxTree>();
                trees.Add( tree );
                StringBuilder bSource = new StringBuilder();
                foreach( var m in Modules)
                {
                    m.AppendSource( bSource );
                    trees.Add( SyntaxFactory.ParseSyntaxTree( bSource.ToString() ) );
                    bSource.Clear();
                    // temporary: allow process of the main (first module) only.
                    trees[0] = m.PostProcess( trees[0] );
                }
                var option = _options.WithAssemblyIdentityComparer(DesktopAssemblyIdentityComparer.Default);
                CSharpCompilation compilation = CSharpCompilation.Create(
                    Path.GetFileNameWithoutExtension(assemblyPath),
                    trees,
                    allReferences,
                    option);

                var r = compilation.Emit(assemblyPath);
                if (r.Success && loader != null)
                {
                    try
                    {
                        return new GenerateResult(null, trees, r, loader(assemblyPath), null, null);
                    }
                    catch (Exception ex)
                    {
                        return new GenerateResult(null, trees, r, null, ex, null);
                    }
                }
                return new GenerateResult(null, trees, r, null, null, null);
            }
            catch (Exception ex)
            {
                return new GenerateResult(ex, null, null, null, null, null);
            }
#if NET461
            }
#endif
        }
    }
}
