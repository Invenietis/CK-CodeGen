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

namespace CK.Text.Tests
{
    static partial class TestHelper
    {
        static string _solutionFolder;

        static TestHelper()
        {
        }

        public static string SolutionFolder
        {
            get
            {
                if (_solutionFolder == null) InitalizePaths();
                return _solutionFolder;
            }
        }

        static void InitalizePaths()
        {
            _solutionFolder = Path.GetDirectoryName(Path.GetDirectoryName(GetTestProjectPath()));
            Console.WriteLine($"SolutionFolder is: {_solutionFolder}.");
        }

        static string GetTestProjectPath([CallerFilePath]string path = null) => Path.GetDirectoryName(path);

    }
}
