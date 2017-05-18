using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using FluentAssertions;
using CK.Core;

namespace CK.CodeGen.Tests
{
    static partial class TestHelper
    {
        static string _solutionFolder;
        static IActivityMonitor _monitor;

        static TestHelper()
        {
            _monitor = new ActivityMonitor();
            _monitor.Output.RegisterClient(new ActivityMonitorConsoleClient());
        }

        public static IActivityMonitor Monitor => _monitor;

        public static string SolutionFolder
        {
            get
            {
                if (_solutionFolder == null) InitalizePaths();
                return _solutionFolder;
            }
        }

        public static string BinFolder => AppContext.BaseDirectory;

        public static string RandomDllPath => Path.Combine(BinFolder, $"Test-{Guid.NewGuid().ToString().Substring(0, 8)}.dll" );

        public static Assembly CreateAssembly(string sourceCode, IEnumerable<Assembly> references)
        {
            return HandleCreateResult(
                sourceCode,
                new CodeGenerator().Generate(sourceCode, TestHelper.RandomDllPath,
                                                Expand(references).Select( a => a.Location ), 
                                                GetAssemblyLoader()));
        }

        static IEnumerable<Assembly> Expand(IEnumerable<Assembly> references )
        {
            HashSet<Assembly> all = new HashSet<Assembly>();
            DoExpand(references, all);
            return all;
        }

        static void DoExpand(IEnumerable<Assembly> references, HashSet<Assembly> all )
        {
            foreach( var a in references)
            {
                if (all.Add(a)) DoExpand(a.GetReferencedAssemblies().Select( n => SafeLoad(n)).Where( x => x != null ), all);
            }
        }

        static Assembly SafeLoad( AssemblyName n )
        {
            Assembly a = null;
            try
            {
                a = Assembly.Load(n);
            }
            catch( Exception )
            {
                try
                {
                    a = Assembly.Load(new AssemblyName(n.Name));
                }
                catch (Exception)   
                {
                }
            }
            return a;
        }

        public static Assembly CreateAssembly(string sourceCode, IEnumerable<string> references)
        {
            return HandleCreateResult(
                sourceCode,
                new CodeGenerator().Generate(sourceCode, TestHelper.RandomDllPath, references, GetAssemblyLoader()));
        }

        public static Assembly CreateAssembly(string sourceCode, IEnumerable<MetadataReference> references)
        {
            return HandleCreateResult(
                sourceCode,
                new CodeGenerator().Generate(sourceCode, TestHelper.RandomDllPath, references, GetAssemblyLoader()));
        }

        private static Assembly HandleCreateResult(string sourceCode, GenerateResult result)
        {
            if (!result.Success)
            {
                Console.WriteLine(sourceCode);
                Console.WriteLine();
                if (!result.EmitResult.Success)
                {
                    foreach (var diag in result.EmitResult.Diagnostics)
                    {
                        Console.WriteLine(diag.ToString());
                    }
                    Console.WriteLine();
                }
                if (result.AssemblyLoadError != null)
                {
                    Console.WriteLine(result.AssemblyLoadError.Message);
                }
            }
            result.Success.Should().BeTrue();
            return result.Assembly;
        }

        private static Func<string, Assembly> GetAssemblyLoader()
        {
            Func<string, Assembly> loader;
#if NET462
            loader = Assembly.LoadFrom;
#else
            loader = System.Runtime.Loader.AssemblyLoadContext.Default.LoadFromAssemblyPath;
#endif
            return loader;
        }

        static void InitalizePaths()
        {
            _solutionFolder = Path.GetDirectoryName(Path.GetDirectoryName(GetTestProjectPath()));
            Console.WriteLine($"SolutionFolder is: {_solutionFolder}.");
        }

        static string GetTestProjectPath([CallerFilePath]string path = null) => Path.GetDirectoryName(path);
    }
}
