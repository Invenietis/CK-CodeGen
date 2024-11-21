namespace CK.CodeGen;

/// <summary>
/// Handles code part composites in a <see cref="IFunctionScope"/>.
/// </summary>
public interface IFunctionScopePart : ICodePart<IFunctionScope>, IFunctionScope
{
    /// <summary>
    /// Creates a segment of code inside this part.
    /// </summary>
    /// <param name="top">
    /// Optionally creates the new part at the start of the code instead of at the
    /// current writing position in the code.
    /// </param>
    /// <returns>The <see cref="IFunctionScopePart"/> part to use.</returns>
    new IFunctionScopePart CreatePart( bool top = false );
}
