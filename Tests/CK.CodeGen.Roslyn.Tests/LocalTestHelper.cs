using CK.Core;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

using static CK.Testing.MonitorTestHelper;

namespace CK.CodeGen.Roslyn.Tests;

static partial class LocalTestHelper
{
    public static string RandomDllPath => TestHelper.BinFolder.AppendPart( $"Test-{Guid.NewGuid().ToString().Substring( 0, 8 )}.dll" );

    public static Assembly CreateAssembly( string sourceCode, IEnumerable<Assembly> references, params ICodeGeneratorModule[] modules )
    {
        var g = new CodeGenerator( CodeWorkspace.Factory );
        g.Modules.AddRange( modules );
        return HandleCreateResult( sourceCode,
                                   g.Generate( sourceCode, RandomDllPath,
                                               references,
                                               false,
                                               GetAssemblyLoader() ) );
    }

    static Assembly HandleCreateResult( string sourceCode, GenerateResult result )
    {
        result.LogResult( TestHelper.Monitor, LogLevel.Info );
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

    static string GetTestProjectPath( [CallerFilePath] string path = null ) => Path.GetDirectoryName( path );
}
