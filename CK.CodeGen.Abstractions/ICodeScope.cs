using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CK.CodeGen.Abstractions
{
    /// <summary>
    /// A code scope is a generic named piece of source code that can be enclosed
    /// in another <see cref="Parent"/> code scope.
    /// It generalizes <see cref="ITypeScope"/> that defines a Type (class, struct, enum, etc.)
    /// and <see cref="INamespaceScope"/> that defines a namespace.
    /// It is itself a <see cref="ICodeWriter"/>: raw code can be appendend to it as needed
    /// that will appear at the top of the final source code.
    /// </summary>
    public interface ICodeScope : ICodeWriter
    {
        /// <summary>
        /// The parent code scope.
        /// This is null for a root <see cref="INamespaceScope"/> (<see cref="INamespaceScope.IsGlobal"/> is true).
        /// </summary>
        ICodeScope Parent { get; }

        /// <summary>
        /// Gets the root workspace.
        /// </summary>
        ICodeWorkspace Workspace { get; }

        /// <summary>
        /// The nam of this code scope: it is the leaf of the <see cref="FullName"/>.
        /// It is never null but can be empty for a root <see cref="INamespaceScope"/> (<see cref="INamespaceScope.IsGlobal"/> is true). 
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The full name of this scope starts with the full name of this <see cref="Parent"/> scope.
        /// It is never null but can be empty for a root <see cref="INamespaceScope"/> (<see cref="INamespaceScope.IsGlobal"/> is true). 
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Creates a <see cref="ITypeScope"/> inside this code scope.
        /// Its name is automatically extracted from the header that may contain the
        /// opening curly brace '{' or not (in such case it is automatically appended).
        /// </summary>
        /// <param name="header">Configure the header. Must not be null.</param>
        /// <returns>The new type scope.</returns>
        ITypeScope CreateType( Action<ITypeScope> header );

        /// <summary>
        /// Finds an existing <see cref="ITypeScope"/> previously created with <see cref="CreateType"/>.
        /// </summary>
        /// <param name="name">The name of the type to find.</param>
        /// <returns>The type or null if not found.</returns>
        ITypeScope FindType( string name );

        /// <summary>
        /// Gets the types that this scope contains.
        /// </summary>
        IReadOnlyCollection<ITypeScope> Types { get; }

        /// <summary>
        /// Collects the whole code into a <see cref="StringBuilder"/>, optionnaly closing the
        /// scope with a trailing '}' or leaving it opened.
        /// </summary>
        /// <param name="b">The string builder to write to.</param>
        /// <param name="closeScope">True to close the scope before returning the builder.</param>
        /// <returns>The string builder.</returns>
        StringBuilder Build( StringBuilder b, bool closeScope );
    }
}
