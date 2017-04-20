using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

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

        public EmitResult Generate(string sourceCode, string assemblyPath, IEnumerable<MetadataReference> references)
        {
            SyntaxTree tree = SyntaxFactory.ParseSyntaxTree(sourceCode);
            var option = _options.WithAssemblyIdentityComparer(DesktopAssemblyIdentityComparer.Default);
            CSharpCompilation compilation = CSharpCompilation.Create(
                Path.GetFileNameWithoutExtension(assemblyPath),
                new[] { tree },
                references,
                option);

            return compilation.Emit(assemblyPath);
        }
    }
}
