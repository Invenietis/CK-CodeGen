using NUnitLite;
using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CK.StObj.Engine.Tests.NetCore
{
    public static class Program
    {
        static string TestPathMap( [CallerFilePath]string p = null ) => p;

        public static int Main(string[] args)
        {
            Console.WriteLine( TestPathMap() );
            CultureInfo.CurrentCulture
                = CultureInfo.CurrentUICulture
                = CultureInfo.DefaultThreadCurrentCulture
                = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo( "en-US" );
            return new AutoRun(Assembly.GetEntryAssembly()).Execute(args);
        }
    }
}
