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

        /// <summary>
        /// Small helper that works like <see cref="HashSet{T}.Add(T)"/> on the <see cref="INamedScope.Memory"/>
        /// dictionary.
        /// </summary>
        /// <param name="this">This scope.</param>
        /// <param name="key">The memory key. Must not be null or empty.</param>
        /// <returns>True if the key is new to the memory (and has been added), false if the key is already known.</returns>
        public static bool MemorizeOnce( this INamedScope @this, string key )
        {
            if( String.IsNullOrEmpty( key ) ) throw new ArgumentException( "Key must not be null or white space.", nameof( key ) );
            if( @this.Memory.ContainsKey( key ) ) return false;
            @this.Memory.Add( key, null );
            return true;
     }

    }
}
