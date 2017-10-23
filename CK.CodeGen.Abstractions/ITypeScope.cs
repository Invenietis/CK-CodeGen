namespace CK.CodeGen.Abstractions
{
    /// <summary>
    /// A type is defined in a <see cref="ITypeDefinerScope"/> and is itself
    /// a type definer and a <see cref="IFunctionDefinerScope"/>.
    /// </summary>
    public interface ITypeScope : ITypeDefinerScope, IFunctionDefinerScope
    {
        /// <summary>
        /// Gets the closest namespace that contains this type.
        /// </summary>
        INamespaceScope Namespace { get; }
    }
}
