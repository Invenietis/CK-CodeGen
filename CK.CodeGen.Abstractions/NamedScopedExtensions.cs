using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen.Abstractions
{
    /// <summary>
    /// Provides extension methods to <see cref="INamedScope"/>.
    /// </summary>
    public static class NamedScopeExtensions
    {
        /// <summary>
        /// Builds and returns the code.
        /// </summary>
        /// <param name="this">This scope.</param>
        /// <param name="closeScope">True to close the scope.</param>
        /// <returns>The source code of this named scope.</returns>
        public static string ToString( this INamedScope @this, bool closeScope )
        {
            return @this.Build( new StringBuilder(), closeScope ).ToString();
        }

    }
}
