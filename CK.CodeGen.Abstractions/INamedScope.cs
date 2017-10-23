using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CK.CodeGen.Abstractions
{
    /// <summary>
    /// A named scope is a generic named piece of source code that can be enclosed
    /// in another <see cref="Parent"/> named scope.
    /// It generalizes <see cref="ITypeScope"/> that defines a Type (class, struct, enum, etc.)
    /// a <see cref="INamespaceScope"/> that defines a namespace or a <see cref="IFunctionScope"/>
    /// for functions or methods.
    /// It is itself a <see cref="ICodeWriter"/>: raw code can be appendend to it as needed
    /// that will appear at the top of the final type, namespace or function source code.
    /// </summary>
    public interface INamedScope : ICodeWriter
    {
        /// <summary>
        /// Gets the root workspace.
        /// </summary>
        ICodeWorkspace Workspace { get; }

        /// <summary>
        /// The parent code scope.
        /// This is null for a root <see cref="INamespaceScope"/>.
        /// </summary>
        INamedScope Parent { get; }

        /// <summary>
        /// The name of this scope: it is the leaf of the <see cref="FullName"/>.
        /// It is never null but can be empty for a global <see cref="INamespaceScope"/>. 
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The full name of this scope starts with the full name of this <see cref="Parent"/> scope.
        /// It is never null but can be empty for a global <see cref="INamespaceScope"/>. 
        /// </summary>
        string FullName { get; }

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
