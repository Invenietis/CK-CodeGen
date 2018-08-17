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

        /// <summary>
        /// Gets this type header that contains the modifiers, attributes, type name and generic constraints
        /// if any.
        /// </summary>
        string TypeHeader { get; }

        /// <summary>
        /// Gets whether this type is defined in another <see cref="ITypeScope"/>
        /// (or in a <see cref="INamespaceScope"/>).
        /// </summary>
        bool IsNestedType { get; }

        /// <summary>
        /// Creates a segment of code inside this type.
        /// </summary>
        /// <returns>The type part to use.</returns>
        ITypeScopePart CreatePart();
    }
}
