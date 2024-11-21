namespace CK.CodeGen;

/// <summary>
/// A code part enables any <see cref="INamedScope"/> to be segmented
/// in as many parts as needed.
/// </summary>
public interface ICodePart<T> : ICodePart where T : INamedScope
{
    /// <summary>
    /// Gets the owner of this part.
    /// </summary>
    T PartOwner { get; }
}
