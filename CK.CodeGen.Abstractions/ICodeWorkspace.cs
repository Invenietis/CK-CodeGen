using System.Collections.Generic;
using System.Reflection;

namespace CK.CodeGen.Abstractions
{
    /// <summary>
    /// Root interface that contains a <see cref="Global"/> <see cref="INamespaceScope"/>
    /// and referneced assemblies.
    /// </summary>
    public interface ICodeWorkspace
    {
        /// <summary>
        /// Gets the global name space.
        /// </summary>
        INamespaceScope Global { get; }

        /// <summary>
        /// Ensures that this workspace references an actual assembly.
        /// </summary>
        /// <param name="assembly">The assembly. Must not be null.</param>
        void DoEnsureAssemblyReference( Assembly assembly );

        /// <summary>
        /// Gets the assemblies that this workspace references.
        /// </summary>
        IReadOnlyCollection<Assembly> AssemblyReferences { get; }

        /// <summary>
        /// Merges the other workspace into this one.
        /// </summary>
        /// <param name="other">Another workspace.</param>
        void MergeWith( ICodeWorkspace other );
    }
}