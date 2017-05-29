using System;
using System.Collections.Generic;
using System.Text;
using CK.Text;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Globalization;
using System.Diagnostics;

namespace CK.CodeGen
{
    public static class CodeGenExtensions
    {
        public static StringBuilder ToCSharpName( this StringBuilder @this, Type t, bool withGenericParamName = false )
        {
            if( t == null ) return @this.Append( "null" );
            if( t.IsGenericParameter ) return withGenericParamName ? @this.Append( t.Name ) : @this;
            var pathTypes = new Stack<Type>();
            pathTypes.Push( t );
            Type decl = t.DeclaringType;
            while( decl != null )
            {
                pathTypes.Push( decl );
                decl = decl.DeclaringType;
            }
            var tInfo = t.GetTypeInfo();
            var allGenArgs = new Queue<Type>( tInfo.GenericTypeArguments );
            foreach( var p in tInfo.GenericTypeParameters ) allGenArgs.Enqueue( p );
            for( int iType = 0; pathTypes.Count > 0; iType++ )
            {
                Type theT = pathTypes.Pop();
                TypeInfo theTInfo = theT.GetTypeInfo();
                string n;
                if( iType == 0 ) n = theT.FullName;
                else
                {
                    n = theT.Name;
                    @this.Append( '.' );
                }
                int idxTick = n.IndexOf( '`' ) + 1;
                if( idxTick > 0 )
                {
                    int endNbParam = idxTick;
                    while( endNbParam < n.Length && Char.IsDigit( n, endNbParam ) ) endNbParam++;
                    int nbParams = int.Parse( n.Substring( idxTick, endNbParam - idxTick ), NumberStyles.Integer );
                    Debug.Assert( nbParams > 0 );
                    @this.Append( n, 0, idxTick - 1 );
                    @this.Append( '<' );
                    for( int iGen = 0; iGen < nbParams; ++iGen )
                    {
                        if( iGen > 0 ) @this.Append( ',' );
                        ToCSharpName( @this, allGenArgs.Dequeue(), withGenericParamName );
                    }
                    @this.Append( '>' );
                }
                else @this.Append( n );
            }
            return @this;
        }

        public static string ToCSharpName( this Type @this, bool withGenericParamName = false )
        {
            return @this == null ? "null" : ToCSharpName( new StringBuilder(), @this, withGenericParamName ).ToString();
        }

        static public string ToGetTypeSourceString( this Type @this )
        {
            return @this == null ? "null" : "Type.GetType(" + @this.AssemblyQualifiedName.ToSourceString() + ')';
        }

        static public string ToSourceString( this string @this )
        {
            return @this == null ? "null" : $"@\"{@this.Replace( "\"", "\"\"" )}\"";
        }

        static public string ToSourceString( this bool @this )
        {
            return @this ? "true" : "false";
        }

        static public StringBuilder ToSourceString(this byte[] @this, StringBuilder b)
        {
            if (@this == null) return b.Append("null");
            if (@this.Length == 0) return b.Append("Array.Empty<byte>()");
            return b.Append("new byte[] {").AppendStrings(@this.Select(x => x.ToString()),",").Append('}');
        }

        static public StringBuilder ToSourceString( this object o, StringBuilder b )
        {
            if( o == null ) return b.Append( "null" );
            if( o == System.Type.Missing ) return  b.Append( "System.Type.Missing" );
            Type oT = o.GetType();
            if( oT.GetTypeInfo().IsValueType )
            {
                if( o is bool ) return b.Append( (bool)o ? "true" : "false" );
                if( o is int ) return b.Append( (int)o );
                if( o is long ) return b.Append("(long)").Append( (long)o );
                if( o is short ) return b.Append("(short)").Append( (short)o );
                if( o is sbyte ) return b.Append( "(sbyte)" ).Append( (sbyte)o );
                if( o is uint ) return b.Append("(uint)").Append( (uint)o );
                if( o is ulong ) return b.Append( "(ulong)" ).Append( (ulong)o );
                if( o is ushort ) return b.Append( "(ushort)" ).Append( (ushort)o );
                if( o is byte ) return b.Append( "(byte)" ).Append( (byte)o );
                if( o is Guid ) return b.Append( "new Guid(\"" ).Append( ((Guid)o).ToString() ).Append( "\")" );
                if( o is char )
                {
                    char c = (char)o;
                    return c == '\\' ? b.Append( @"'\\'" ) : b.Append( '\'' ).Append( (char)o ).Append( '\'' );
                }
                if( o is double ) return b.Append( ((double)o).ToString(CultureInfo.InvariantCulture) );
                if( o is float ) return b.Append( ((float)o ).ToString( CultureInfo.InvariantCulture ) ).Append('f');
                if( o is decimal ) return b.Append( ((decimal)o).ToString( CultureInfo.InvariantCulture ) ).Append('m');
                if( o is DateTime )
                {
                    DateTime d = (DateTime)o;
                    return b.Append( "new DateTime(" ).Append( d.Ticks ).Append( ", DateTimeKind." ).Append( d.Kind ).Append( ')' );
                }
                if( o is TimeSpan )
                {
                    TimeSpan ts = (TimeSpan)o;
                    return b.Append( "new TimeSpan(" ).Append( ts.Ticks ).Append( ')' );
                }
                if( o is DateTimeOffset )
                {
                    DateTimeOffset to = (DateTimeOffset)o;
                    return b.Append( "new DateTimeOffset(" ).Append( to.Ticks ).Append( ", new TimeSpan(" ).Append( to.Offset.Ticks ).Append( "))" );
                }
                throw new ArgumentException( "Unknown value type: " + oT.AssemblyQualifiedName );
            }
            string s = o as string;
            if( s != null ) return b.Append( s.ToSourceString() );
            Type t = o as Type;
            if( t != null ) return b.Append( "Type.GetType(" ).Append( t.AssemblyQualifiedName.ToSourceString() ).Append(')');
            byte[] bA = o as byte[];
            if( bA != null ) return bA.ToSourceString( b );
            // TODO: Handle IEnumerable<T> to generate typed array.
            // Last try: mere IEnumerable.
            IEnumerable e = o as IEnumerable;
            if( e != null )
            {
                var i = e.GetEnumerator();
                bool any = i.MoveNext();
                (i as IDisposable)?.Dispose();
                if( any )
                {
                    b.Append( "new object[] {" );
                    bool existing = false;
                    foreach( var x in e )
                    {
                        if( existing ) b.Append( ',' );
                        else existing = true;
                        ToSourceString( x, b );
                    }
                    return b.Append( '}' );
                }
                return b.Append( "Array.Empty<object>()" );
            }
            else throw new ArgumentException( "Unknown type: " + oT.AssemblyQualifiedName );
        }

        static public StringBuilder AppendSourceString(this StringBuilder @this, byte[] b) => ToSourceString(b, @this);

        static public StringBuilder AppendSourceString(this StringBuilder @this, string s) => @this.Append(ToSourceString(s));

    }


}
