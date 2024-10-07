using System;
using System.Collections.Generic;

namespace CK.CodeGen;

/// <summary>
/// Handles code part composites in a <see cref="ITypeScopePart"/>.
/// </summary>
public interface ITypeScopePart : ICodePart<ITypeScope>, ITypeScope
{
    /// <summary>
    /// Creates a segment of code inside this part.
    /// </summary>
    /// <param name="top">
    /// Optionally creates the new part at the start of the code instead of at the
    /// current writing position in the code.
    /// </param>
    /// <returns>The <see cref="ITypeScopePart"/> part to use.</returns>
    new ITypeScopePart CreatePart( bool top = false );
}
