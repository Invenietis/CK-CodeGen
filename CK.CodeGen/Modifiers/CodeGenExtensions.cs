using System;
using System.Collections.Generic;
using System.Text;
using CK.Text;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Globalization;
using System.Diagnostics;
using CK.CodeGen.Abstractions;

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

        public static ICodeWriter AppendCSharpName( this ICodeWriter @this, Type t, bool typeDeclaration = true )
        {
            if( t == null ) return @this.Append( "null" );
            if( t.IsGenericParameter ) return typeDeclaration ? @this.Append( t.Name ) : @this;
            string alias;
            if( _typeAliases.TryGetValue( t, out alias ) )
            {
                return @this.Append( alias );
            }
            if( t == typeof( void ) ) return @this.Append( "void" );
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

        static public string ToGetTypeSourceString( this Type @this )
        {
            return @this == null
                    ? "null"
                    : (@this != typeof( void )
                        ? "Type.GetType(" + @this.AssemblyQualifiedName.ToSourceString() + ')'
                        : "typeof(void)");
        }

        static public string ToSourceString( this string @this ) => @this == null ? "null" : $"@\"{@this.Replace( "\"", "\"\"" )}\"";

        static public string ToSourceString( this bool @this ) => @this ? "true" : "false";

        static public ICodeWriter AppendSourceString( this ICodeWriter @this, bool b ) => @this.Append( b ? "true" : "false" );

        static public ICodeWriter AppendSourceString<T>( this ICodeWriter @this, IEnumerable<T> e )
        {
            if( e == null ) return @this.Append( "null" );
            if( !e.Any() ) return @this.Append( "Array.Empty<" ).AppendCSharpName( typeof( T ), false ).Append( ">()" );
            @this.Append( "new " ).AppendCSharpName( typeof( T ), false ).Append( "[]{" );
            bool already = false;
            foreach( var x in e )
            {
                if( already ) @this.Append( ',' );
                else already = true;
                AppendSourceString( @this, x );
            }
            return @this.Append( '}' );
        }

        static public ICodeWriter AppendSourceString( this ICodeWriter @this, char c )
        {
            return c == '\\' ? @this.Append( @"'\\'" ) : @this.Append( '\'' ).Append( c ).Append( '\'' );
        }

        static public ICodeWriter AppendSourceString( this ICodeWriter @this, int i ) => @this.Append( i );

        static public ICodeWriter AppendSourceString( this ICodeWriter @this, long i ) => @this.Append( "(long)" ).Append( i );

        static public ICodeWriter AppendSourceString( this ICodeWriter @this, ulong i ) => @this.Append( "(ulong)" ).Append( i );

        static public ICodeWriter AppendSourceString( this ICodeWriter @this, short i ) => @this.Append( "(short)" ).Append( i );

        static public ICodeWriter AppendSourceString( this ICodeWriter @this, ushort i ) => @this.Append( "(ushort)" ).Append( i );

        static public ICodeWriter AppendSourceString( this ICodeWriter @this, sbyte i ) => @this.Append( "(sbyte)" ).Append( i );

        static public ICodeWriter AppendSourceString( this ICodeWriter @this, byte i ) => @this.Append( "(byte)" ).Append( i );

        static public ICodeWriter AppendSourceString( this ICodeWriter @this, uint i ) => @this.Append( "(uint)" ).Append( i );

        static public ICodeWriter AppendSourceString( this ICodeWriter @this, Guid g ) => @this.Append( "new Guid(\"" ).Append( g.ToString() ).Append( "\")" );

        static public ICodeWriter AppendSourceString( this ICodeWriter @this, double d ) => @this.Append( d.ToString( CultureInfo.InvariantCulture ) );

        static public ICodeWriter AppendSourceString( this ICodeWriter @this, float f ) => @this.Append( f.ToString( CultureInfo.InvariantCulture ) ).Append( 'f' );

        static public ICodeWriter AppendSourceString( this ICodeWriter @this, decimal d ) => @this.Append( d.ToString( CultureInfo.InvariantCulture ) ).Append( 'm' );

        static public ICodeWriter AppendSourceString( this ICodeWriter @this, DateTime d )
        {
            return @this.Append( "new DateTime(" ).Append( d.Ticks ).Append( ", DateTimeKind." ).Append( d.Kind ).Append( ')' );
        }

        static public ICodeWriter AppendSourceString( this ICodeWriter @this, TimeSpan ts )
        {
            return @this.Append( "new TimeSpan(" ).Append( ts.Ticks ).Append( ')' );
        }

        static public ICodeWriter AppendSourceString( this ICodeWriter @this, DateTimeOffset to )
        {
            return @this.Append( "new DateTimeOffset(" ).Append( to.Ticks ).Append( ", new TimeSpan(" ).Append( to.Offset.Ticks ).Append( "))" );
        }

        static public ICodeWriter AppendSourceString( this ICodeWriter @this, IEnumerable e )
        {
            if( e == null ) return @this.Append( "null" );
            Type type = typeof( object );
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

        static public ICodeWriter AppendSourceString( this ICodeWriter @this, string s ) => @this.Append( ToSourceString( s ) );

        static public ICodeWriter AppendSourceString( this ICodeWriter @this, object o )
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

        public static ITypeScope AppendFrontModifier( this ITypeScope @this, string modifier ) => @this.AppendWithWhitespace( modifier );

        public static ITypeScope DefineKind( this ITypeScope @this, string kind ) => @this.AppendWithWhitespace( kind );

        public static ITypeScope DefineName( this ITypeScope @this, string name ) => @this.AppendWithWhitespace( name );

        public static ITypeScope SetBase( this ITypeScope @this, Type baseType )
        {
            @this.Append(':').AppendCSharpName( baseType, true );
            return @this;
        }

        public static T AppendWithWhitespace<T>( this T @this, object arg ) where T : ICodeWriter => @this.Append( arg ).Append( ' ' );

        public static T Append<T>( this T @this, char c ) where T : ICodeWriter
        {
            @this.RawAppend( c.ToString() );
            return @this;
        }

        public static T Append<T>( this T @this, object o ) where T : ICodeWriter
        {
            @this.RawAppend( o.ToString() );
            return @this;
        }

        public static ICodeWriter Append( this ICodeWriter @this, string s, int startIndex, int count )
        {
            @this.RawAppend( s.Substring( startIndex, count ) );
            return @this;
        }

        public static ITypeScope DefineOverrideMethod( this ITypeScope @this, MethodInfo baseMethod, Action<ICodeWriter> bodyBuilder )
        {
            if( baseMethod == null ) throw new ArgumentNullException( nameof( baseMethod ) );
            string name = baseMethod.Name;
            if( baseMethod.ContainsGenericParameters )
            {
                name += '<';
                name += baseMethod.GetGenericArguments().Select( a => a.Name ).Concatenate();
                name += '>';
            }
            List<string> frontModifiers = new List<string>();
            ModifierHelper.AddFrontModifiersProtection( baseMethod, frontModifiers );
            frontModifiers.Add( "override" );
            foreach( string frontModifier in frontModifiers ) @this.AppendWithWhitespace( frontModifier );
            @this.AppendCSharpName( baseMethod.ReturnType );
            @this.RawAppend( name );

            @this.AddParameters( baseMethod );

            @this.AppendLine().AppendLine( '{' );
            bodyBuilder?.Invoke( @this );
            @this.AppendLine().AppendLine( '}' );
            return @this;
        }

        static void AddParameters( this ICodeWriter @this, MethodInfo baseMethod )
        {
            @this.Append( '(' );
            bool isFirstParameter = true;
            foreach( var p in baseMethod.GetParameters() )
            {
                @this.AddParameter( p );
                if( isFirstParameter ) isFirstParameter = false;
                else @this.AppendWithWhitespace( ',' );
            }
            @this.Append( ')' );
        }

        static void AddParameter( this ICodeWriter @this, ParameterInfo p )
        {
            if( p.IsOut ) @this.AppendWithWhitespace( "out" );
            else if( p.ParameterType.IsByRef ) @this.AppendWithWhitespace( "ref" );
            Type parameterType = p.ParameterType.IsByRef ? p.ParameterType.GetElementType() : p.ParameterType;
            @this.AppendCSharpName( parameterType, true );
            @this.RawAppend( p.Name );
        }

        public static ICodeWriter AppendLine( this ICodeWriter @this )
        {
            @this.RawAppend( Environment.NewLine );
            return @this;
        }

        public static ICodeWriter AppendLine( this ICodeWriter @this, object arg )
        {
            @this.Append( arg );
            @this.RawAppend( Environment.NewLine );
            return @this;
        }

        public static ICodeWriter DefinePassThroughConstructors( this ICodeWriter @this, Type baseType )
        {
            return @this;
        }

        public static ICodeWriter DefineOverrideMethod( this ICodeWriter @this, MethodInfo method, Action<ICodeWriter> bodyBuilder )
        {
            return @this;
        }
    }
}
