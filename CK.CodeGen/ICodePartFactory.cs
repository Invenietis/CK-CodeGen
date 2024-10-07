namespace CK.CodeGen;

/// <summary>
/// Abstraction required to unify parts.
/// </summary>
public interface ICodePartFactory
{
    /// <summary>
    /// Creates a segment of code inside this code.
    /// </summary>
    /// <param name="top">
    /// Optionally creates the new part at the start of the code instead of at the
    /// current writing position in the code.
    /// </param>
    /// <returns>The code part to use.</returns>
    ICodePart CreatePart( bool top = false );
}
