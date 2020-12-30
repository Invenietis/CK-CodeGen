using System;
using System.Collections.Generic;

namespace CK.CodeGen
{
    /// <summary>
    /// Handles code part composites in a <see cref="ITypeScopePart"/>.
    /// </summary>
    public interface ITypeScopePart : ICodePart<ITypeScope>, ITypeScope
    {
    }
}
