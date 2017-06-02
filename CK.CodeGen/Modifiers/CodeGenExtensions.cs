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
        static readonly Dictionary<Type, string> _typeAliases;

        static CodeGenExtensions()
        {
            _typeAliases = new Dictionary<Type, string>();
            _typeAliases.Add( typeof( void ), "void" );
            _typeAliases.Add( typeof( bool ), "bool" );
            _typeAliases.Add( typeof( int ), "int" );
            _typeAliases.Add( typeof( long ), "long" );
            _typeAliases.Add( typeof( short ), "short" );
            _typeAliases.Add( typeof( ushort ), "ushort" );
            _typeAliases.Add( typeof( sbyte ), "sbyte" );
            _typeAliases.Add( typeof( uint ), "uint" );
            _typeAliases.Add( typeof( ulong ), "ulong" );
            _typeAliases.Add( typeof( byte ), "byte" );
            _typeAliases.Add( typeof( char ), "char" );
            _typeAliases.Add( typeof( double ), "double" );
            _typeAliases.Add( typeof( float ), "float" );
            _typeAliases.Add( typeof( decimal ), "decimal" );
            _typeAliases.Add( typeof( string ), "string" );
            _typeAliases.Add( typeof( object ), "object" );
        }

        public static StringBuilder AppendCSharpName( this StringBuilder @this, Type t, bool typeDeclaration = true )
        {
            if( t == null ) return @this.Append( "null" );
            if( t.IsGenericParameter ) return typeDeclaration ? @this.Append( t.Name ) : @this;
            string alias;
            if( _typeAliases.TryGetValue(t,out alias))
            {
                return @this.Append( alias );
            }
            if( t == typeof(void) ) return @this.Append( "void" );
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
                        AppendCSharpName( @this, allGenArgs.Dequeue(), typeDeclaration );
                    }
                    @this.Append( '>' );
                }
                else @this.Append( n );
            }
            return @this;
        }

        public static string ToCSharpName( this Type @this, bool typeDeclaration = true )
        {
            return @this == null ? "null" : AppendCSharpName( new StringBuilder(), @this, typeDeclaration ).ToString();
        }

        static public string ToGetTypeSourceString( this Type @this )
        {
            return @this == null 
                    ? "null" 
                    : (@this != typeof(void)
                        ? "Type.GetType(" + @this.AssemblyQualifiedName.ToSourceString() + ')'
                        : "typeof(void)");
        }

        static public string ToSourceString( this string @this ) => @this == null ? "null" : $"@\"{@this.Replace( "\"", "\"\"" )}\"";

        static public string ToSourceString( this bool @this ) => @this ? "true" : "false";

        static public StringBuilder AppendSourceString( this StringBuilder @this, bool b ) => @this.Append( b ? "true" : "false" );

        static public StringBuilder AppendSourceString<T>( this StringBuilder @this, IEnumerable<T> e )
        {
            if( e == null ) return @this.Append( "null" );
            if( !e.Any() ) return @this.Append( "Array.Empty<").AppendCSharpName(typeof(T),false).Append( ">()" );
            @this.Append( "new " ).AppendCSharpName(typeof(T),false).Append("[]{" );
            bool already = false;
            foreach( var x in e )
            {
                if( already ) @this.Append( ',' );
                else already = true;
                AppendSourceString( @this, x );
            }
            return @this.Append( '}' );
        }

        static public StringBuilder AppendSourceString( this StringBuilder @this, char c )
        {
            return c == '\\' ? @this.Append( @"'\\'" ) : @this.Append( '\'' ).Append( c ).Append( '\'' );
        }

        static public StringBuilder AppendSourceString( this StringBuilder @this, int i ) => @this.Append( i );

        static public StringBuilder AppendSourceString( this StringBuilder @this, long i ) => @this.Append( "(long)" ).Append( i );

        static public StringBuilder AppendSourceString( this StringBuilder @this, ulong i ) => @this.Append( "(ulong)" ).Append( i );

        static public StringBuilder AppendSourceString( this StringBuilder @this, short i ) => @this.Append( "(short)" ).Append( i );

        static public StringBuilder AppendSourceString( this StringBuilder @this, ushort i ) => @this.Append( "(ushort)" ).Append( i );

        static public StringBuilder AppendSourceString( this StringBuilder @this, sbyte i ) => @this.Append( "(sbyte)" ).Append( i );

        static public StringBuilder AppendSourceString( this StringBuilder @this, byte i ) => @this.Append( "(byte)" ).Append( i );

        static public StringBuilder AppendSourceString( this StringBuilder @this, uint i ) => @this.Append( "(uint)" ).Append( i );

        static public StringBuilder AppendSourceString( this StringBuilder @this, Guid g ) => @this.Append( "new Guid(\"" ).Append( g.ToString() ).Append( "\")" );

        static public StringBuilder AppendSourceString( this StringBuilder @this, double d ) => @this.Append( d.ToString( CultureInfo.InvariantCulture ) );

        static public StringBuilder AppendSourceString( this StringBuilder @this, float f ) => @this.Append( f.ToString( CultureInfo.InvariantCulture ) ).Append( 'f' );

        static public StringBuilder AppendSourceString( this StringBuilder @this, decimal d ) => @this.Append( d.ToString( CultureInfo.InvariantCulture ) ).Append( 'm' );

        static public StringBuilder AppendSourceString( this StringBuilder @this, DateTime d )
        {
            return @this.Append( "new DateTime(" ).Append( d.Ticks ).Append( ", DateTimeKind." ).Append( d.Kind ).Append( ')' );
        }

        static public StringBuilder AppendSourceString( this StringBuilder @this, TimeSpan ts )
        {
            return @this.Append( "new TimeSpan(" ).Append( ts.Ticks ).Append( ')' );
        }

        static public StringBuilder AppendSourceString( this StringBuilder @this, DateTimeOffset to )
        {
            return @this.Append( "new DateTimeOffset(" ).Append( to.Ticks ).Append( ", new TimeSpan(" ).Append( to.Offset.Ticks ).Append( "))" );
        }

        static public StringBuilder AppendSourceString( this StringBuilder @this, IEnumerable e )
        {
            if( e == null ) return @this.Append( "null" );
            Type type = typeof(object);
            var eI = e.GetType()
                        .GetTypeInfo()
                        .ImplementedInterfaces
                        .FirstOrDefault( iT => iT.GetTypeInfo().IsGenericType && iT.GetGenericTypeDefinition() == typeof( IEnumerable<> ) );
            if( eI != null )
            {
                var arg = eI.GetTypeInfo().GenericTypeArguments;
                if( arg.Length == 1 ) type = arg[0];
            }
            var i = e.GetEnumerator();
            bool any = i.MoveNext();
            (i as IDisposable)?.Dispose();
            if( any )
            {
                @this.Append( "new " ).AppendCSharpName( type, false ).Append( "[]{" );
                bool existing = false;
                foreach( var x in e )
                {
                    if( existing ) @this.Append( ',' );
                    else existing = true;
                    AppendSourceString( @this, x );
                }
                return @this.Append( '}' );
            }
            return @this.Append( "Array.Empty<" ).AppendCSharpName( type, false ).Append( ">()" );
        }

        static public StringBuilder AppendSourceString(this StringBuilder @this, string s) => @this.Append(ToSourceString(s));

        static public StringBuilder AppendSourceString( this StringBuilder @this, object o )
        {
            if( o == null ) return @this.Append( "null" );
            if( o == System.Type.Missing ) return @this.Append( "System.Type.Missing" );
            TypeInfo oT = o.GetType().GetTypeInfo();
            if( oT.IsValueType )
            {
                if( o is bool ) return AppendSourceString( @this, (bool)o );
                if( o is int ) return AppendSourceString( @this, (int)o );
                if( o is long ) return AppendSourceString( @this, (long)o );
                if( o is short ) return AppendSourceString( @this, (short)o );
                if( o is ushort ) return AppendSourceString( @this, (ushort)o );
                if( o is sbyte ) return AppendSourceString( @this, (sbyte)o );
                if( o is uint ) return AppendSourceString( @this, (uint)o );
                if( o is ulong ) return AppendSourceString( @this, (ulong)o );
                if( o is byte ) return AppendSourceString( @this, (byte)o );
                if( o is Guid ) return AppendSourceString( @this, (Guid)o );
                if( o is char ) return AppendSourceString( @this, (char)o );
                if( o is double ) return AppendSourceString( @this, (double)o );
                if( o is float ) return AppendSourceString( @this, (float)o );
                if( o is decimal ) return AppendSourceString( @this, (decimal)o );
                if( o is DateTime ) return AppendSourceString( @this, (DateTime)o );
                if( o is TimeSpan ) return AppendSourceString( @this, (TimeSpan)o );
                if( o is DateTimeOffset ) return AppendSourceString( @this, (DateTimeOffset)o );
            }
            else
            {
                string s = o as string;
                if( s != null ) return @this.Append( s.ToSourceString() );
                Type t = o as Type;
                if( t != null ) return @this.Append( "typeof(" ).AppendCSharpName( t, false ).Append( ')' ); 
                IEnumerable e = o as IEnumerable;
                if( e != null ) return AppendSourceString( @this, e );
            }

            throw new ArgumentException( "Unknown type: " + oT.AssemblyQualifiedName );
        }

    }


}
