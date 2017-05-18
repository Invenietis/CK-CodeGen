using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;

namespace CK.CodeGen
{
    public class CodeGenerator
    {
        readonly CSharpCompilationOptions _options;

        public CodeGenerator()
            : this(null)
        {
        }

        public CodeGenerator(CSharpCompilationOptions options)
        {
            if (options == null) options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary);
            _options = options;
        }

        /// <summary>
        /// Generates an assembly from a source code and a list of absolute path to required reference assemblies.
        /// </summary>
        /// <param name="sourceCode">The source code. Must be vali C# code.</param>
        /// <param name="assemblyPath">The full final assemblt path (including the .dll extension).</param>
        /// <param name="refAssemblyPaths">List of full paths to reference assemblies.</param>
        /// <param name="loader">Optional loader function.</param>
        /// <returns>Encapsulation of the result.</returns>
        public GenerateResult Generate(string sourceCode, string assemblyPath, IEnumerable<string> refAssemblyPaths, Func<string, Assembly> loader = null)
        {
            try
            {
                return Generate(sourceCode, assemblyPath, refAssemblyPaths.Select(p => MetadataReference.CreateFromFile(p)), loader);
            }
            catch( Exception ex )
            {
                return new GenerateResult(ex, null, null, null);
            }
        }

        public GenerateResult Generate(string sourceCode, string assemblyPath, IEnumerable<MetadataReference> references, Func<string, Assembly> loader = null)
        {
            try
            {
                SyntaxTree tree = SyntaxFactory.ParseSyntaxTree(sourceCode);
                var option = _options.WithAssemblyIdentityComparer(DesktopAssemblyIdentityComparer.Default);
                CSharpCompilation compilation = CSharpCompilation.Create(
                    Path.GetFileNameWithoutExtension(assemblyPath),
                    new[] { tree },
                    references,
                    option);

                var r = compilation.Emit(assemblyPath);
                if (r.Success && loader != null)
                {
                    try
                    {
                        return new GenerateResult(null, r, loader(assemblyPath), null);
                    }
                    catch (Exception ex)
                    {
                        return new GenerateResult(null, r, null, ex);
                    }
                }
                return new GenerateResult(null, r, null, null);
            }
            catch (Exception ex)
            {
                return new GenerateResult(ex, null, null, null);
            }
        }
    }
}
