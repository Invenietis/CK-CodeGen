using CK.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen;

/// <summary>
/// Provides extension methods to <see cref="ITypeDefinerScope"/>.
/// </summary>
public static class TypeDefinerExtensions
{
    /// <summary>
    /// Creates a <see cref="ITypeScope"/> inside this scope.
    /// Its name is automatically extracted from the header that may contain the
    /// opening curly brace '{' or not (in such case it is automatically appended).
    /// </summary>
    /// <param name="this">This scope.</param>
    /// <param name="header">The header of the type. Must not be null.</param>
    /// <returns>The new type scope.</returns>
    public static ITypeScope CreateType( this ITypeDefinerScope @this, string header )
    {
        Throw.CheckNotNullArgument( header );
        Throw.CheckNotNullArgument( @this );
        return @this.CreateType( t => t.Append( header ) );
    }

}
