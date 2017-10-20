using System;
using System.Reflection;
using System.Threading;

namespace CK.Core
{

    /// <summary>
    /// Assemby loader helper: hooks the <see cref="AppDomain.AssemblyResolve"/> event
    /// in order to try to load a version less assembly.
    /// All memebers are thread safe.
    /// </summary>
    public static class WeakAssemblyNameResolver
    {
        static int _installCount;

        /// <summary>
        /// Gets whether this helper is active.
        /// </summary>
        public static bool IsInstalled => _installCount >= 0;

        /// <summary>
        /// Installs the hook if not already installed.
        /// </summary>
        public static void Install()
        {
            if( Interlocked.Increment( ref _installCount ) == 1 )
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            }
        }

        /// <summary>
        /// Uninstall the hook if possible.
        /// </summary>
        public static void Uninstall()
        {
            if( Interlocked.Decrement( ref _installCount ) == 0 )
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
            return failed.Version != null && string.IsNullOrWhiteSpace( failed.CultureName )
                    ? Assembly.Load( new AssemblyName( failed.Name ) )
                    : null;
        }
    }
}
