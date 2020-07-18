using System;
using System.Collections.Generic;

namespace CK.CodeGen
{
    /// <summary>
    /// Handles code part composites in a <see cref="INamespaceScope"/>.
    /// </summary>
    public interface INamespaceScopePart : ICodePart<INamespaceScope>, INamespaceScope
    {
    }
}
