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
    /// </summary>
    public interface INamedScope 
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
        /// Collects the whole code into a string collector, optionnaly closing the
        /// scope with a trailing '}' or leaving it opened.
        /// </summary>
        /// <param name="collector">The string collector to write to.</param>
        /// <param name="closeScope">True to close the scope.</param>
        void Build( Action<string> collector, bool closeScope );

        /// <summary>
        /// Collects the whole code into a <see cref="StringBuilder"/>, optionnaly closing the
        /// scope with a trailing '}' or leaving it opened.
        /// </summary>
        /// <param name="b">The string builder to write to.</param>
        /// <param name="closeScope">True to close the scope before returning the builder.</param>
        /// <returns>The string builder.</returns>
        StringBuilder Build( StringBuilder b, bool closeScope );

        /// <summary>
        /// Gets a memory associated to this scope.
        /// It can contain any data that need to be associated to this scope.
        /// </summary>
        IDictionary<object, object> Memory { get; }

    }
}
