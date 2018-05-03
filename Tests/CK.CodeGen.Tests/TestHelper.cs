using System;
using System.IO;
using System.Runtime.CompilerServices;
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

        static void InitalizePaths()
        {
            _solutionFolder = Path.GetDirectoryName( Path.GetDirectoryName( GetTestProjectPath() ) );
            Console.WriteLine( $"SolutionFolder is: {_solutionFolder}." );
        }

        static string GetTestProjectPath( [CallerFilePath]string path = null ) => Path.GetDirectoryName( path );
    }
}
