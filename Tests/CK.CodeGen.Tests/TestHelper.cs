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
            _monitor.Output.RegisterClient( new ActivityMonitorConsoleClient() );
        }

        public static IActivityMonitor Monitor => _monitor;

        public static string SolutionFolder
        {
            get
            {
                if( _solutionFolder == null ) InitalizePaths();
                return _solutionFolder;
            }
        }

        public static string BinFolder => AppContext.BaseDirectory;

        public static string RandomDllPath => Path.Combine( BinFolder, $"Test-{Guid.NewGuid().ToString().Substring( 0, 8 )}.dll" );

        public static Assembly CreateAssembly( string sourceCode, IEnumerable<Assembly> references, params ICodeGeneratorModule[] modules )
        {
            var g = new CodeGenerator();
            g.Modules.AddRange( modules );
            return HandleCreateResult(
                sourceCode,
                g.Generate( sourceCode, TestHelper.RandomDllPath,
                            references, DefaultAssemblyResolver.Default,
                            GetAssemblyLoader() ) );
        }


        public static Assembly CreateAssembly( string sourceCode, IEnumerable<string> references )
        {
            return HandleCreateResult(
                sourceCode,
                new CodeGenerator().Generate( sourceCode, TestHelper.RandomDllPath, references, GetAssemblyLoader() ) );
        }

        public static Assembly CreateAssembly( string sourceCode, IEnumerable<MetadataReference> references )
        {
            return HandleCreateResult(
                sourceCode,
                new CodeGenerator().Generate( sourceCode, TestHelper.RandomDllPath, references, GetAssemblyLoader() ) );
        }

        static Assembly HandleCreateResult( string sourceCode, GenerateResult result )
        {
            result.LogResult( TestHelper.Monitor );
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
            _solutionFolder = Path.GetDirectoryName( Path.GetDirectoryName( GetTestProjectPath() ) );
            Console.WriteLine( $"SolutionFolder is: {_solutionFolder}." );
        }

        static string GetTestProjectPath( [CallerFilePath]string path = null ) => Path.GetDirectoryName( path );
    }
}
