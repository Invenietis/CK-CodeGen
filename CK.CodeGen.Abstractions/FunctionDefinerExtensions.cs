using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CK.CodeGen.Abstractions
{
    /// <summary>
    /// Provides extension methods to <see cref="IFunctionDefinerScope"/>.
    /// </summary>
    public static class FunctionDefinerExtensions
    {        
        /// <summary>
        /// Creates a <see cref="IFunctionScope"/> inside this scope.
        /// Its name is automatically extracted from the header that may contain the
        /// opening curly brace '{' or not (in such case it is automatically appended).
        /// </summary>
        /// <param name="this">This scope.</param>
        /// <param name="header">The header of the function. Must not be null.</param>
        /// <returns>The new function scope.</returns>
        public static IFunctionScope CreateFunction( this IFunctionDefinerScope @this, string header )
        {
            if( header == null ) throw new ArgumentNullException( nameof( header ) );
            return @this.CreateFunction( t => t.Append( header ) );
        }

    }
}
