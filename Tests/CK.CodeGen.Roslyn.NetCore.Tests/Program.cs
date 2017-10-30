using NUnitLite;
using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CK.CodeGen.Roslyn.Tests
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            CultureInfo.CurrentCulture
                = CultureInfo.CurrentUICulture
                = CultureInfo.DefaultThreadCurrentCulture
                = CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo( "en-US" );
            return new AutoRun(Assembly.GetEntryAssembly()).Execute(args);
        }
    }
}
