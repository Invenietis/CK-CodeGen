using System;
using System.Collections.Generic;

namespace CK.CodeGen.Abstractions
{
    /// <summary>
    /// Handles code part composites in a <see cref="ITypeScopePart"/>.
    /// </summary>
    public interface ITypeScopePart : ICodePart<ITypeScope>, ITypeScope
    {
        /// <summary>
        /// Creates a subordinated part of code in this part of code.
        /// </summary>
        /// <returns>The type part to use.</returns>
        ITypeScopePart CreateSubPart();
    }
}
