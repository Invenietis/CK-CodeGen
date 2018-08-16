using System;
using System.Collections.Generic;

namespace CK.CodeGen.Abstractions
{
    /// <summary>
    /// Handles code part composites in a <see cref="INamespaceScope"/>.
    /// </summary>
    public interface INamespaceScopePart : ICodePart<INamespaceScope>, INamespaceScope
    {
        /// <summary>
        /// Creates a part of code in this part of code.
        /// </summary>
        /// <returns>The namespace part to use.</returns>
        INamespaceScopePart CreateSubPart();
    }
}
