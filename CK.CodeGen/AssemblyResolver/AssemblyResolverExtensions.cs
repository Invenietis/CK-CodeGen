using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CK.CodeGen
{
    /// <summary>
    /// Extends <see cref="IAssemblyResolver"/> with the recursive discovering of
    /// referenced assemblies.
    /// </summary>
    public static class AssemblyResolverExtensions
    {
        class Resolver
        {
            readonly IAssemblyResolver _resolver;
            readonly HashSet<Assembly> _all;
            readonly List<AssemblyLoadFailure> _failures;

            public Resolver( IAssemblyResolver resolver )
            {
                _resolver = resolver;
                _all = new HashSet<Assembly>();
                _failures = new List<AssemblyLoadFailure>();
            }

            public Resolver Process( Assembly a )
            {
                if( a != null && _all.Add( a ) )
                {
                    foreach( var d in _resolver.GetReferencedAssemblies( a ).Select( n => SafeLoad( n ) ) )
                    {
                        if( d != null ) Process( d );
                    }
                }
                return this;
            }

            public AssemblyClosureResult Result => new AssemblyClosureResult( _failures, _all );

            Assembly SafeLoad( AssemblyName n )
            {
                Assembly a = null;
                try
                {
                    a = _resolver.LoadByName( n );
                    if( a.FullName != n.FullName )
                    {
                        _failures.Add( new AssemblyLoadFailure( n, a.GetName() ) );
                    }
                }
                catch
                {
                    if( n.Version != null && String.IsNullOrWhiteSpace( n.CultureName ) )
                    {
                        var uName = new AssemblyName( n.Name );
                        try
                        {
                            a = Assembly.Load( uName );
                            _failures.Add( new AssemblyLoadFailure( n, a.GetName() ) );
                            return a;
                        }
                        catch
                        {
                        }
                        _failures.Add( new AssemblyLoadFailure( n, null ) );
                    }
                }
                return a;
            }
        }

        /// <summary>
        /// Gets the transitive dependencies of one assembly.
        /// </summary>
        /// <param name="this">This assembly resolver.</param>
        /// <param name="a">The assembly.</param>
        /// <returns>A result with the closure or errors.</returns>
        public static AssemblyClosureResult GetAssemblyClosure( this IAssemblyResolver @this, Assembly a )
        {
            return new Resolver( @this ).Process( a ).Result;
        }

        /// <summary>
        /// Gets the transitive dependencies of a set of assemblies.
        /// </summary>
        /// <param name="this">This assembly resolver.</param>
        /// <param name="assemblies">The assemblies.</param>
        /// <returns>A result with the closure or errors.</returns>
        public static AssemblyClosureResult GetAssembliesClosure( this IAssemblyResolver @this, IEnumerable<Assembly> assemblies )
        {
            var r = new Resolver( @this );
            foreach( var a in assemblies ) r.Process( a );
            return r.Result;
        }

    }
}
