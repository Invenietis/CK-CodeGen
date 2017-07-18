#if NET461
using System;
using System.Reflection;
using System.Threading;

namespace CK.Core
{
    public static class WeakAssemblyNameResolver
    {
        static int _installCount;

        public static bool IsInstalled => _installCount >= 0;

        public static void Install()
        {
            if( Interlocked.Increment(ref _installCount) == 1 )
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            }
        }

        public static void Uninstall()
        {
            if (Interlocked.Decrement(ref _installCount) == 0)
            {
                AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
            }
        }

        class Auto : IDisposable
        {
            bool _done;

            public void Dispose()
            {
                if( !_done )
                {
                    _done = true;
                    Uninstall();
                }
            }
        }

        /// <summary>
        /// Temporary installs the hook that will be uninstalled when the returned object will be disposed.
        /// </summary>
        /// <returns>The dispoable to dispose when done.</returns>
        public static IDisposable TempInstall()
        {
            Install();
            return new Auto();
        }

        static Assembly CurrentDomain_AssemblyResolve( object sender, ResolveEventArgs args )
        {
            var failed = new AssemblyName( args.Name );
            //This does not work either.
            //if( failed.Name == "System.IO.FileSystem" && failed.Version == new Version( 4, 0, 1, 0 ) )
            //{
            //    return typeof( System.IO.FileStream ).Assembly;
            //}
            return failed.Version != null && failed.CultureName == null
                    ? Assembly.Load( new AssemblyName( failed.Name ) )
                    : null;
        }
    }
}
#endif