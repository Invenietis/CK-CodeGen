using System.Collections.Generic;
using System.Reflection;

namespace CK.CodeGen.Abstractions
{
    /// <summary>
    /// Root interface that contains a <see cref="Global"/> <see cref="INamespaceScope"/>.
    /// </summary>
    public interface ICodeWorkspace
    {
        /// <summary>
        /// Gets the global name space.
        /// </summary>
        INamespaceScope Global { get; }

        /// <summary>
        /// Ensures that this code scope will reference an actual assembly.
        /// </summary>
        /// <param name="assembly">The assembly. Must not be null.</param>
        /// <returns>This codes scope to enable fluent syntax.</returns>
        ICodeWorkspace EnsureAssemblyReference( Assembly assembly );
    }
}
