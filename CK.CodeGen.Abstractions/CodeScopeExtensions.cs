using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen.Abstractions
{
    /// <summary>
    /// Provides extension methods to <see cref="ICodeScope"/>.
    /// </summary>
    public static class CodeScopeExtensions
    {        
        /// <summary>
        /// Creates a <see cref="ITypeScope"/> inside this code scope.
        /// Its name is automatically extracted from the header that may contain the
        /// opening curly brace '{' or not (in such case it is automatically appended).
        /// </summary>
        /// <param name="this">This code scope.</param>
        /// <param name="header">The header of the type. Must not be null.</param>
        /// <returns>The new type scope.</returns>
        public static ITypeScope CreateType( this ICodeScope @this, string header )
        {
            if( header == null ) throw new ArgumentNullException( nameof( header ) );
            return @this.CreateType( t => t.Append( header ) );
        }

    }
}
