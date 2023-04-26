using CK.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen
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
            Throw.CheckNotNullArgument( @this );
            return @this.Build( new StringBuilder(), closeScope ).ToString();
        }

        /// <summary>
        /// Small helper that works like <see cref="HashSet{T}.Add(T)"/> on the <see cref="INamedScope.Memory"/>
        /// dictionary.
        /// </summary>
        /// <param name="this">This scope.</param>
        /// <param name="key">The memory key. Must not be null or empty.</param>
        /// <returns>True if the key is new to the memory (and has been added), false if the key is already known.</returns>
        public static bool MemorizeOnce( this INamedScope @this, string key )
        {
            Throw.CheckNotNullArgument( @this );
            Throw.CheckNotNullOrEmptyArgument( key );
            return @this.Memory.TryAdd( key, null );
        }

    }
}
