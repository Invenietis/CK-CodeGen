namespace CK.CodeGen
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
        /// Gets this type definition.
        /// Except the <see cref="TypeDefinition.Name"/> that uniquely identifies this type in the <see cref="Namespace"/>,
        /// this type definition can be mutated.
        /// </summary>
        TypeDefinition TypeDefinition { get; }

        /// <summary>
        /// Gets a unique incremental identifier for this type.
        /// The first value is 1.
        /// <para>
        /// This is useful to generate a unique (typically short) name that
        /// identifies this type, regardless of its full namespace and name.
        /// </para>
        /// <para>
        /// Caution: This identifier is provided by the <see cref="ICodeWorkspace"/>: when <see cref="ICodeWorkspace.MergeWith(ICodeWorkspace)"/>
        /// is used and the same type is defined in both of them, the primary workspace (the one that is merged with the other) keeps
        /// its own unique identifier.
        /// </para>
        /// </summary>
        int UniqueId { get; }

        /// <summary>
        /// Gets whether this type is defined in another <see cref="ITypeScope"/>
        /// (or in a <see cref="INamespaceScope"/>).
        /// </summary>
        bool IsNestedType { get; }

        /// <summary>
        /// Creates a segment of code inside this type.
        /// </summary>
        /// <param name="top">
        /// Optionally creates the new part at the start of the code instead of at the
        /// current writing position in the code.
        /// </param>
        /// <returns>The type part to use.</returns>
        ITypeScopePart CreatePart( bool top = false );
    }
}
