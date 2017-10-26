using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.CodeGen
{
    enum Modifiers
    {
        None = 0,
        Readonly = 1,
        Static = 1 << 1,
        Internal = 1 << 2,
        Private = 1 << 3,
        Protected = 1 << 4,
        Public = 1 << 5,
        Abstract = 1 << 6,
        New = 1 << 7,
        Virtual = 1 << 8,
        Sealed = 1 << 9,
        Override = 1 << 10,
        Explicit = 1 << 11,
        Extern = 1 << 12,
        Implicit = 1 << 13,
        Unsafe = 1 << 14,
        Volatile = 1 << 15,
        Async = 1 << 16
    }

    static class ModifiersExtension
    {
        static readonly Dictionary<string, int> _map = ((int[])Enum.GetValues( typeof( Modifiers ) ))
                                                        .Where( v => v != 0 )
                                                        .ToDictionary( v => Enum.GetName( typeof( Modifiers ), v ).ToLowerInvariant() );
        static readonly string[] _names = _map.OrderBy( kv => kv.Value ).Select( kv => kv.Key ).ToArray();

        internal static bool Combine( this Modifiers @this, string m )
        {
            if( !_map.TryGetValue( m, out var v ) ) return false;
            @this |= (Modifiers)v;
            return true;
        }

        internal static Modifiers NormalizeForType( this Modifiers @this )
        {
            return @this &
                    (
                        Modifiers.Public | Modifiers.Protected | Modifiers.Private | Modifiers.Internal
                        | Modifiers.Abstract
                        | Modifiers.Sealed
                        | Modifiers.Static
                    );
        }

        internal static Modifiers NormalizeNamespaceProtection( this Modifiers @this )
        {
            return @this & ~(Modifiers.Protected | Modifiers.Private | Modifiers.Internal);
        }

        internal static Modifiers NormalizeMemberProtection( this Modifiers @this )
        {
            if( (@this & Modifiers.Public) != 0 )
            {
                @this = @this & ~(Modifiers.Private | Modifiers.Internal | Modifiers.Protected);
            }
            else if( (@this & Modifiers.Private) != 0 )
            {
                @this = @this & ~(Modifiers.Public | Modifiers.Internal);
            }
            else if( (@this & Modifiers.Protected) != 0 )
            {
                @this = @this & ~(Modifiers.Public);
            }
            return @this;
        }

        static public StringBuilder Write( this Modifiers @this, StringBuilder b )
        {
            for( int i = 0; i <_names.Length; ++i )
            {
                if( ((int)@this & (1 << i)) != 0 ) b.Append( _names[i] ).Append( ' ' ); 
            }
            return b;
        }
    }


}
