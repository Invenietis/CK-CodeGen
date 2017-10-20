using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace CK.CodeGen
{
    /// <summary>
    /// Captures the result of the <see cref="AssemblyResolverExtensions.GetAssemblyClosure(IAssemblyResolver, Assembly)"/>
    /// method.
    /// </summary>
    public class AssemblyClosureResult
    {
        /// <summary>
        /// The detailed loadind errors or warnings.
        /// </summary>
        public readonly IReadOnlyCollection<AssemblyLoadFailure> LoadFailures;

        /// <summary>
        /// The set of all the assemblies.
        /// </summary>
        public readonly IEnumerable<Assembly> AllAssemblies;

        /// <summary>
        /// Initializes a new <see cref="AssemblyClosureResult"/>.
        /// </summary>
        /// <param name="f">Failures (errors or warnings).</param>
        /// <param name="a">Set of resolved assemblies.</param>
        public AssemblyClosureResult( IReadOnlyCollection<AssemblyLoadFailure> f, IEnumerable<Assembly> a )
        {
            LoadFailures = f;
            AllAssemblies = a;
        }

    }
}
