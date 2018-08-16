using System;
using System.Collections.Generic;

namespace CK.CodeGen.Abstractions
{
    /// <summary>
    /// Handles code part composites in a <see cref="IFunctionScope"/>.
    /// </summary>
    public interface IFunctionScopePart : ICodePart<IFunctionScope>, IFunctionScope
    {
        /// <summary>
        /// Creates a part of code in this part of code.
        /// </summary>
        /// <returns>The function part to use.</returns>
        IFunctionScopePart CreateSubPart();
    }
}
