using System;
using System.Collections.Generic;

namespace CK.CodeGen.Abstractions
{
    /// <summary>
    /// Handles code part composites in a <see cref="IFunctionScope"/>.
    /// </summary>
    public interface IFunctionScopePart : ICodePart<IFunctionScope>, IFunctionScope
    {
    }
}
