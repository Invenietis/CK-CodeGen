namespace CK.CodeGen.Abstractions
{
    /// <summary>
    /// A type is defined in a <see cref="ICodeScope"/> that can
    /// be a <see cref="INamespaceScope"/> or a <see cref="ITypeScope"/>.
    /// </summary>
    public interface ITypeScope : ICodeScope
    {
        /// <summary>
        /// Gets the closest namespace that contains this type.
        /// </summary>
        INamespaceScope Namespace { get; }
    }
}
