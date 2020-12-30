using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.CodeGen
{
    /// <summary>
    /// Extends the <see cref="Modifiers"/> enumeration.
    /// </summary>
    public static class ModifiersExtension
    {
        static readonly Dictionary<string, int> _map = ((int[])Enum.GetValues( typeof( Modifiers ) ))
                                                        .Where( v => v != 0 )
                                                        .ToDictionary( v => Enum.GetName( typeof( Modifiers ), v ).ToLowerInvariant() );
        static readonly string[] _names = _map.OrderBy( kv => kv.Value ).Select( kv => kv.Key ).ToArray();

        /// <summary>
        /// Parses one of the <see cref="Modifiers"/> and or'ed it with the given one.
        /// </summary>
        /// <param name="m">The current modifiers value.</param>
        /// <param name="text">One potential modifier.</param>
        /// <returns>True on success, false otherwise.</returns>
        public static bool ParseAndCombine( ref Modifiers m, string text )
        {
            if( !_map.TryGetValue( text, out var v ) ) return false;
            m |= (Modifiers)v;
            return true;
        }

        /// <summary>
        /// Keeps only flags that apply to type.
        /// </summary>
        /// <param name="this">This modifiers.</param>
        /// <returns>The updated modifiers.</returns>
        public static Modifiers NormalizeForType( this Modifiers @this )
        {
            return @this &
                    (
                        Modifiers.Public | Modifiers.Protected | Modifiers.Private | Modifiers.Internal
                        | Modifiers.Abstract
                        | Modifiers.Sealed
                        | Modifiers.Static
                        | Modifiers.Ref | Modifiers.Readonly
                        | Modifiers.Data
                    );
        }

        /// <summary>
        /// Normalizes namesapce access protection: "private", "internal" and "protected" are cleared.
        /// </summary>
        /// <param name="this">This modifiers.</param>
        /// <returns>The updated modifiers.</returns>
        public static Modifiers NormalizeNamespaceProtection( this Modifiers @this )
        {
            return @this & ~(Modifiers.Protected | Modifiers.Private | Modifiers.Internal);
        }

        /// <summary>
        /// Normalizes member access protection: when "public" is specified, "private", "internal" and "protected" are cleared.
        /// Moreover, "private" alone is cleared.
        /// </summary>
        /// <param name="this">This modifiers.</param>
        /// <returns>The updated modifiers.</returns>
        public static Modifiers NormalizeMemberProtection( this Modifiers @this )
        {
            if( (@this & Modifiers.Public) != 0 )
            {
                @this = @this & ~(Modifiers.Private | Modifiers.Internal | Modifiers.Protected);
            }
            if( @this == Modifiers.Private ) @this = Modifiers.None;
            return @this;
        }

        /// <summary>
        /// Writes this Modifiers into the provided StringBuilder.
        /// </summary>
        /// <param name="this">This modifiers.</param>
        /// <param name="b">The target.</param>
        /// <returns>The StringBuilder to enable fluent syntax.</returns>
        public static StringBuilder Write( this Modifiers @this, StringBuilder b )
        {
            for( int i = 0; i <_names.Length; ++i )
            {
                if( ((int)@this & (1 << i)) != 0 ) b.Append( _names[i] ).Append( ' ' ); 
            }
            return b;
        }
    }


}
