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
                                                references, DefaultAssemblyResolver.Default, 
                                                GetAssemblyLoader()));
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

        static Assembly HandleCreateResult(string sourceCode, GenerateResult result)
        {
            using (Monitor.OpenInfo().Send("Code Generation information."))
            {
                if( result.LoadFailures.Count > 0 )
                {
                    using (Monitor.OpenWarn().Send($"{result.LoadFailures.Count} assembly load failure(s)."))
                        foreach (var e in result.LoadFailures)
                            if (e.SuccessfulWeakFallback != null) Monitor.Warn().Send($"'{e.Name}' load failed, used '{e.SuccessfulWeakFallback}' instead.");
                            else Monitor.Error().Send($"'{e.Name}' load failed.");
                }
                if (!result.Success)
                {
                    using (Monitor.OpenError().Send("Generation failed."))
                    {
                        if( result.EmitError != null )
                        {
                            Monitor.Error().Send(result.EmitError);
                        }
                        if (!result.EmitResult.Success && !result.EmitResult.Diagnostics.IsEmpty)
                        {
                            using (Monitor.OpenError().Send("Compilation diagnostics."))
                            {
                                foreach (var diag in result.EmitResult.Diagnostics)
                                {
                                    Monitor.Trace().Send(diag.ToString());
                                }
                            }
                        }
                    }
                }
                if (result.AssemblyLoadError != null)
                {
                    Monitor.Error().Send(result.AssemblyLoadError, "Generated assembly load failed." );
                }
                else if(result.Assembly != null)
                {
                    Monitor.Trace().Send("Generated assembly successfuly loaded.");
                }
            }
            result.Success.Should().BeTrue();
            return result.Assembly;
        }

        private static Func<string, Assembly> GetAssemblyLoader()
        {
            Func<string, Assembly> loader;
#if NET461
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
