using System;
using System.Collections.Generic;

namespace CK.CodeGen;

/// <summary>
/// A type definer scope is a generic named piece of source code that can be enclosed
/// in another <see cref="INamedScope.Parent">Parent</see> code scope.
/// It generalizes <see cref="ITypeScope"/> that defines a Type (class, struct, enum, etc.)
/// a <see cref="INamespaceScope"/> that defines a namespace or a <see cref="IFunctionScope"/>
/// for functions or methods.
/// It is itself a <see cref="ICodeWriter"/>: raw code can be appended to it as needed
/// that will appear at the top of the final type, namespace or function source code.
/// </summary>
public interface ITypeDefinerScope : INamedScope, ICodeWriter
{
    /// <summary>
    /// Creates a <see cref="ITypeScope"/> inside this scope.
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
    ITypeScope? FindType( string name );

    /// <summary>
    /// Gets the types that this scope contains.
    /// </summary>
    IReadOnlyCollection<ITypeScope> Types { get; }

}
