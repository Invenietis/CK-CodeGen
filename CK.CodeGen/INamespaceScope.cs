using System;
using System.Collections.Generic;

namespace CK.CodeGen
{
    /// <summary>
    /// A namespace is a <see cref="ITypeDefinerScope"/> (it can define types).
    /// It handles "using" clauses and creates subordinated namespaces.
    /// </summary>
    public interface INamespaceScope : ITypeDefinerScope, ICodePartFactory
    {
        /// <summary>
        /// Gets the parent namespace.
        /// Null when this namespace is the <see cref="ICodeWorkspace.Global"/> namespace.
        /// </summary>
        new INamespaceScope? Parent { get; }

        /// <summary>
        /// Ensures that a using is defined in this type scope or above.
        /// If it is not defined here or above, it is added to this namespace.
        /// Using statements are written right after the namespace declaration.
        /// </summary>
        /// <param name="ns">The namespace name.</param>
        /// <returns>This namespace to enable fluent syntax.</returns>
        INamespaceScope EnsureUsing( string ns );

        /// <summary>
        /// Gets a basic part that addresses the space before this namespace.
        /// It is the top of the whole source when this namespace is the <see cref="ICodeWorkspace.Global"/> one,
        /// otherwise it is located in the parent namespace.
        /// </summary>
        ICodePart BeforeNamespace { get; }

        /// <summary>
        /// Ensures that a using alias is defined in this type scope with the given definition.
        /// If it is not defined here, nor defined above with the same definition, it is
        /// added to this namespace.
        /// If an alias is already defined in this namespace but with a different definition
        /// a <see cref="ArgumentException"/> is thrown.
        /// </summary>
        /// <param name="alias">The alias name. Can not be null or empty.</param>
        /// <param name="definition">The definition. Can not be null or empty nor ends with the ';'.</param>
        /// <returns>This namespace to enable fluent syntax.</returns>
        INamespaceScope EnsureUsingAlias( string alias, string definition );

        /// <summary>
        /// Ensures that a subordinate namespace exists (can be a composite name).
        /// </summary>
        /// <param name="ns">The (potentially composite) namespace name. Must not be null or empty.</param>
        /// <returns>The newly created or existing namespace.</returns>
        INamespaceScope FindOrCreateNamespace( string ns );

        /// <summary>
        /// Gets the list of direct subordinated namespaces.
        /// </summary>
        IReadOnlyCollection<INamespaceScope> Namespaces { get; }

        /// <summary>
        /// Creates a segment of code inside this namespace.
        /// </summary>
        /// <param name="top">
        /// Optionally creates the new part at the start of the code instead of at the
        /// current writing position in the code.
        /// </param>
        /// <returns>The namespace part to use.</returns>
        new INamespaceScopePart CreatePart( bool top = false );
        
    }
}
