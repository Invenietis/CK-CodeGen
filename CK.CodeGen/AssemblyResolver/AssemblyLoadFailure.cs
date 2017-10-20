using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace CK.CodeGen
{
    /// <summary>
    /// Captures information related to assembly load failures.
    /// </summary>
    public struct AssemblyLoadFailure
    {
        /// <summary>
        /// The assembly name that failed.
        /// </summary>
        public readonly AssemblyName Name;

        /// <summary>
        /// The assembly name that has been successfully used: when not null, its version is
        /// not the same as the original <see cref="Name"/>.
        /// </summary>
        public readonly AssemblyName SuccessfulWeakFallback;

        /// <summary>
        /// If <see cref="SuccessfulWeakFallback"/> is not null, the <see cref="Name"/> has been
        /// eventually resolved to another assembly version.
        /// </summary>
        public bool IsWarning => SuccessfulWeakFallback != null;

        /// <summary>
        /// Initializes a new <see cref="AssemblyLoadFailure"/>.
        /// </summary>
        /// <param name="n">Name of the requested assembly that failed.</param>
        /// <param name="nWeak">Weak name that has been successfully loaded.</param>
        public AssemblyLoadFailure( AssemblyName n, AssemblyName nWeak )
        {
            Name = n;
            SuccessfulWeakFallback = nWeak;
        }
    }
}
