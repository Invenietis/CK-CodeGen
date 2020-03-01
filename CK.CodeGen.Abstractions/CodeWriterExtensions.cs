using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Globalization;
using System.Diagnostics;
using CK.CodeGen.Abstractions;

namespace CK.CodeGen
{
    /// <summary>
    /// Provides Append fluent extension methods to <see cref="ICodeWriter"/> specializations.
    /// </summary>
    public static class CodeWriterExtensions
    {
        static readonly Dictionary<Type, string> _typeAliases;

        static CodeWriterExtensions()
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

        /// <summary>
        /// Appends raw C# code only once: the code itself is used as a key in <see cref="INamedScope.Memory"/> to
        /// avoid adding it twice.
        /// </summary>
        /// <typeparam name="T">Must be both a <see cref="ICodeWriter"/> and <see cref="INamedScope"/>.</typeparam>
        /// <param name="this">This named scope and code writer.</param>
        /// <param name="code">Raw code to append. Must not be null, empty or white space.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T AppendOnce<T>( this T @this, string code ) where T : ICodeWriter, INamedScope
        {
            if( String.IsNullOrWhiteSpace( code ) ) throw new ArgumentException( "To guaranty AppendOnce semantics, code must not be null or white space.", nameof( code ) );
            if( !@this.Memory.ContainsKey( code ) )
            {
                @this.Append( code );
                @this.Memory.Add( code, null );
            }
            return @this;
        }

        /// <summary>
        /// Appends raw C# code.
        /// This is the most basic Append method to use.
        /// Use <see cref="AppendSourceString{T}(T, string)"/> to append the source string representation.
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="code">Raw code to append.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T Append<T>( this T @this, string code ) where T : ICodeWriter
        {
            @this.DoAdd( code );
            return @this;
        }

        /// <summary>
        /// Appends raw character.
        /// Use <see cref="AppendSourceChar{T}(T, char)"/> to append the source string representation
        /// of the character.
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="c">Char to append.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T Append<T>( this T @this, char c ) where T : ICodeWriter
        {
            @this.DoAdd( c.ToString() );
            return @this;
        }

        /// <summary>
        /// Appends a white space.
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T Space<T>( this T @this ) where T : ICodeWriter => @this.Append( " " );

        /// <summary>
        /// Appends a <see cref="Environment.NewLine"/>.
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T NewLine<T>( this T @this ) where T : ICodeWriter => @this.Append( Environment.NewLine );

        /// <summary>
        /// Appends the C# type name. Handles generic definition (either opened or closed).
        /// The <paramref name="typeDeclaration"/> parameters applies to open generics:
        /// When true (the default), typeof( Dictionary&lt;,&gt;.KeyCollection )
        /// will append "System.Collections.Generic.Dictionary&lt;TKey,TValue&gt;.KeyCollection".
        /// When sets to false, it will append "System.Collections.Generic.Dictionary&lt;,&gt;.KeyCollection".
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="t">The type to append.</param>
        /// <param name="typeDeclaration">True to include generic parameter names in the output.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        public static T AppendCSharpName<T>( this T @this, Type t, bool typeDeclaration = true ) where T : ICodeWriter
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
            var allGenArgs = new Queue<Type>( t.GetGenericArguments() );
            for( int iType = 0; pathTypes.Count > 0; iType++ )
            {
                Type theT = pathTypes.Pop();
                string n;
                if( iType == 0 ) n = theT.FullName;
                else
                {
                    n = theT.Name;
                    @this.Append( "." );
                }
                int idxTick = n.IndexOf( '`' ) + 1;
                if( idxTick > 0 )
                {
                    int endNbParam = idxTick;
                    while( endNbParam < n.Length && Char.IsDigit( n, endNbParam ) ) endNbParam++;
                    int nbParams = int.Parse( n.Substring( idxTick, endNbParam - idxTick ), NumberStyles.Integer );
                    Debug.Assert( nbParams > 0 );
                    @this.Append( n.Substring( 0, idxTick - 1 ) );
                    @this.Append( "<" );
                    for( int iGen = 0; iGen < nbParams; ++iGen )
                    {
                        if( iGen > 0 ) @this.Append( "," );
                        AppendCSharpName( @this, allGenArgs.Dequeue(), typeDeclaration );
                    }
                    @this.Append( ">" );
                }
                else @this.Append( n );
            }
            return @this;
        }


        /// <summary>
        /// Appends "typeof(<see cref="AppendCSharpName"/>)" with the type name in is non declaration form:
        /// for the open generic dictionary this is "typeof(System.Collections.Generic.Dictionary&lt;,&gt;)".
        /// When <paramref name="t"/> is null, null is appended.
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="t">The type to append with typeof operator. When null, "null" will be appended.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        public static T AppendTypeOf<T>( this T @this, Type t ) where T : ICodeWriter
        {
            return t == null
                    ? @this.Append( "null" )
                    : @this.Append( "typeof(" ).AppendCSharpName( t, false ).Append( ")" );
        }

        /// <summary>
        /// Appends either "true" or "false".
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="b">The boolean value.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T Append<T>( this T @this, bool b ) where T : ICodeWriter => @this.Append( b ? "true" : "false" );

        /// <summary>
        /// Appends the source representation of an integer value.
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="i">The integer.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T Append<T>( this T @this, int i ) where T : ICodeWriter
        {
            return @this.Append( i.ToString( CultureInfo.InvariantCulture ) );
        }

        /// <summary>
        /// Appends the source representation of a long value (0 is appended as "0L").
        /// </summary>
        /// <param name="this">This code writer.</param>
        /// <param name="i">The long.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T Append<T>( this T @this, long i ) where T : ICodeWriter
        {
            return @this.Append( i.ToString( CultureInfo.InvariantCulture ) ).Append( "L" );
        }

        /// <summary>
        /// Appends the source representation of a <see cref="UInt64"/> value (0 is appended as "(ulong)0").
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="i">The unsigned long.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T Append<T>( this T @this, ulong i ) where T : ICodeWriter
        {
            return @this.Append( "(ulong)" ).Append( i.ToString( CultureInfo.InvariantCulture ) );
        }

        /// <summary>
        /// Appends the source representation of a <see cref="Int16"/> value (0 is appended as "(short)0").
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="i">The short integer..</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T Append<T>( this T @this, short i ) where T : ICodeWriter
        {
            return @this.Append( "(short)" ).Append( i.ToString( CultureInfo.InvariantCulture ) );
        }

        /// <summary>
        /// Appends the source representation of a <see cref="UInt16"/> value (0 is appended as "(ushort)0").
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="i">The unsigned short integer..</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T Append<T>( this T @this, ushort i ) where T : ICodeWriter
        {
            return @this.Append( "(ushort)" ).Append( i.ToString( CultureInfo.InvariantCulture ) );
        }

        /// <summary>
        /// Appends the source representation of a <see cref="SByte"/> value (0 is appended as "(sbyte)0").
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="i">The signed byte.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T Append<T>( this T @this, sbyte i ) where T : ICodeWriter
        {
            return @this.Append( "(sbyte)" ).Append( i.ToString( CultureInfo.InvariantCulture ) );
        }
        /// <summary>
        /// Appends the source representation of a <see cref="Byte"/> value (0 is appended as "(byte)0").
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="i">The byte.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T Append<T>( this T @this, byte i ) where T : ICodeWriter
        {
            return @this.Append( "(byte)" ).Append( i.ToString( CultureInfo.InvariantCulture ) );
        }

        /// <summary>
        /// Appends the source representation of a <see cref="UInt32"/> value (0 is appended as "(uint)0").
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="i">The unsigned integer.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T Append<T>( this T @this, uint i ) where T : ICodeWriter
        {
            return @this.Append( "(uint)" ).Append( i.ToString( CultureInfo.InvariantCulture ) );
        }

        /// <summary>
        /// Appends the source representation of a <see cref="Guid"/> value: "new Guid(...)".
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="g">The Guid.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T Append<T>( this T @this, Guid g ) where T : ICodeWriter
        {
            return @this.Append( "new Guid(\"" ).Append( g.ToString() ).Append( "\")" );
        }

        /// <summary>
        /// Appends the source representation of a <see cref="Double"/> value.
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="d">The double.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T Append<T>( this T @this, double d ) where T : ICodeWriter
        {
            return @this.Append( d.ToString( CultureInfo.InvariantCulture ) );
        }

        /// <summary>
        /// Appends the source representation of a <see cref="Single"/> value.
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="f">The float.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T Append<T>( this T @this, float f ) where T : ICodeWriter
        {
            return @this.Append( f.ToString( CultureInfo.InvariantCulture ) ).Append( "f" );
        }

        /// <summary>
        /// Appends the source representation of a <see cref="Decimal"/> value (0 is appended as "0m").
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="d">The decimal.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T Append<T>( this T @this, decimal d ) where T : ICodeWriter
        {
            return @this.Append( d.ToString( CultureInfo.InvariantCulture ) ).Append( "m" );
        }

        /// <summary>
        /// Appends the source representation of a <see cref="DateTime"/> value: "new DateTime( ... )".
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="d">The datetime.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T Append<T>( this T @this, DateTime d ) where T : ICodeWriter
        {
            return @this.Append( "new DateTime(" ).Append( d.Ticks ).Append( ", DateTimeKind." ).Append( d.Kind.ToString() ).Append( ")" );
        }

        /// <summary>
        /// Appends the source representation of a <see cref="TimeSpan"/> value: "new TimeSpan( ... )".
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="ts">The time span.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T Append<T>( this T @this, TimeSpan ts ) where T : ICodeWriter
        {
            return @this.Append( "new TimeSpan(" ).Append( ts.Ticks.ToString( CultureInfo.InvariantCulture ) ).Append( ")" );
        }

        /// <summary>
        /// Appends the source representation of a <see cref="DateTimeOffset"/> value: "new DateTimeOffset( ... )".
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="to">The date time with offset.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T Append<T>( this T @this, DateTimeOffset to ) where T : ICodeWriter
        {
            return @this.Append( "new DateTimeOffset(" )
                        .Append( to.Ticks )
                        .Append( ", new TimeSpan(" )
                        .Append( to.Offset.Ticks )
                        .Append( "))" );
        }

        /// <summary>
        /// Appends the source representation of a character: "'c'".
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="c">The character.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T AppendSourceChar<T>( this T @this, char c ) where T : ICodeWriter
        {
            switch( c )
            {
                case '\\': return @this.Append( @"'\\'" );
                case '\r': return @this.Append( @"'\r'" );
                case '\n': return @this.Append( @"'\n'" );
                case '\t': return @this.Append( @"'\t'" );
                case '\0': return @this.Append( @"'\0'" );
                case '\b': return @this.Append( @"'\b'" );
                case '\v': return @this.Append( @"'\v'" );
                case '\a': return @this.Append( @"'\a'" );
                case '\f': return @this.Append( @"'\f'" );
            }
            int vC = c;
            if( vC < 32
                || (vC >= 127 && vC <= 160)
                || vC >= 888 )
            {
                return @this.Append( "'\\u" ).Append( vC.ToString( "X4" ) ).Append( "'" );
            }
            return @this.Append( "'" ).Append( c.ToString() ).Append( "'" );
        }

        /// <summary>
        /// Appends the source representation of the string.
        /// See <see cref="ExternalTypeExtensions.ToSourceString(string)"/>.
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="s">The string. Can be null.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T AppendSourceString<T>( this T @this, string s ) where T : ICodeWriter
        {
            return @this.Append( s.ToSourceString() );
        }

        /// <summary>
        /// Appends multiple string (raw C# code) at once, separated with a comma by default.
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="strings">The string. Can be null or empty.</param>
        /// <param name="separator">Separator between the strings.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T Append<T>( this T @this, IEnumerable<string> strings, string separator = ", " ) where T : ICodeWriter
        {
            if( strings != null )
            {
                if( String.IsNullOrEmpty( separator ) ) separator = null;
                bool already = false;
                foreach( var s in strings )
                {
                    if( already )
                    {
                        if( separator != null ) @this.Append( separator );
                    }
                    else already = true;
                    Append( @this, s );
                }
            }
            return @this;
        }

        /// <summary>
        /// Appends the code of a collection of objetcs of a given type <typeparamref name="T"/>.
        /// The code is either "null", <see cref="Array.Empty{T}()"/> or an actual new array
        /// with the items appended with <see cref="Append{T}(T, object)"/>: only
        /// basic types are supported.
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <typeparam name="TItem">The items type.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="e">Set of items for which code must be generated. Can be null.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T AppendCollection<T,TItem>( this T @this, IEnumerable<TItem> e ) where T : ICodeWriter
        {
            if( e == null ) return @this.Append( "null" );
            if( !e.Any() ) return @this.Append( "Array.Empty<" ).AppendCSharpName( typeof( TItem ), false ).Append( ">()" );
            @this.Append( "new " ).AppendCSharpName( typeof( TItem ), false ).Append( "[]{" );
            bool already = false;
            foreach( TItem x in e )
            {
                if( already ) @this.Append( "," );
                else already = true;
                Append( @this, x );
            }
            return @this.Append( "}" );
        }

        /// <summary>
        /// Appends the code of a collection of objects.
        /// If the actual set is a <see cref="IEnumerable{T}"/>, the actual type is extracted
        /// otherwise the type of the items is considered as being object.
        /// The code is either "null", <see cref="Array.Empty{T}()"/> or an actual new array
        /// with the items appended with <see cref="Append{T}(T, object)"/>: only
        /// basic types are supported.
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="e">Set of items for which code must be generated. Can be null.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T AppendCollection<T>( this T @this, IEnumerable e ) where T : ICodeWriter
        {
            if( e == null ) return @this.Append( "null" );
            Type type = typeof( object );
            var eI = e.GetType()
                        .GetTypeInfo()
                        .ImplementedInterfaces
                        .FirstOrDefault( iT => iT.IsGenericType && iT.GetGenericTypeDefinition() == typeof( IEnumerable<> ) );
            if( eI != null )
            {
                type = eI.GetGenericArguments()[0];
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
                    if( existing ) @this.Append( "," );
                    else existing = true;
                    Append( @this, x );
                }
                return @this.Append( "}" );
            }
            return @this.Append( "Array.Empty<" ).AppendCSharpName( type, false ).Append( ">()" );
        }

        /// <summary>
        /// Appends the code source of an enumeration value.
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <typeparam name="E">Type of the <see cref="Enum"/>.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="o">The enum value.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T Append<T, E>( this T @this, E o ) where T : ICodeWriter where E : Enum => AppendEnumValue( @this, typeof( E ), o );

        static T AppendEnumValue<T>( T @this, Type t, object o ) where T : ICodeWriter
        {
            @this.Append( "((" ).Append( t.FullName ).Append( ')' );
            char tU = Enum.GetUnderlyingType( t ).Name[0];
            if( tU == 'U' || tU == 'B' )
            {
                // An enum based on byte (enum EByte : byte) or any other unsigned integral type shorter than a ulong
                // cannot be cast into a ulong...
                @this.Append( Convert.ToUInt64( o ) );
            }
            else
            {
                long v = Convert.ToInt64( o );
                if( v >= 0 ) @this.Append( v );
                else @this.Append( '(' ).Append( v ).Append( ')' );
            }
            return @this.Append( ')' );
        }

        /// <summary>
        /// Appends the code source for an untyped object.
        /// Only types that are implemented throug one of the existing Append extension methods
        /// are supported: an <see cref="ArgumentException"/> is thrown for unsuported type.
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="o">The object. Can be null.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T Append<T>( this T @this, object o ) where T : ICodeWriter
        {
            if( o == Type.Missing ) return @this.Append( "System.Type.Missing" );
            if( o == DBNull.Value ) return @this.Append( "System.DBNull.Value" );           
            switch( o )
            {
                case null: return @this.Append( "null" );
                case Type x: return @this.Append( "typeof(" ).AppendCSharpName( x, false ).Append( ")" );
                case string x: return Append( @this, x.ToSourceString() );
                case bool x: return Append( @this, x );
                case int x: return Append( @this, x );
                case long x: return Append( @this, x );
                case short x: return Append( @this, x );
                case ushort x: return Append( @this, x );
                case sbyte x: return Append( @this, x );
                case uint x: return Append( @this, x );
                case ulong x: return Append( @this, x );
                case byte x: return Append( @this, x );
                case Guid x: return Append( @this, x );
                case char x: return AppendSourceChar( @this, x );
                case double x: return Append( @this, x );
                case float x: return Append( @this, x );
                case Decimal x: return Append( @this, x );
                case DateTime x: return Append( @this, x );
                case TimeSpan x: return Append( @this, x );
                case DateTimeOffset x: return Append( @this, x );
                case IEnumerable x: return AppendCollection( @this, x );
            }
            Type t = o.GetType();
            if( t.IsEnum ) return AppendEnumValue( @this, t, o );
            throw new ArgumentException( "Unknown type: " + o.GetType().AssemblyQualifiedName );
        }

        /// <summary>
        /// Creates a segment of code inside this function.
        /// This signature allows a fluent code to "emit" one or more insertion points.
        /// </summary>
        /// <typeparam name="T">The function scope type.</typeparam>
        /// <param name="this">This function scope.</param>
        /// <param name="part">The function part to use to inject code at this location (or at the top).</param>
        /// <param name="top">Optionally creates the new part at the start of the code instead of at the current writing position in the code.</param>
        /// <returns>This function scope writer to enable fluent syntax.</returns>
        public static T CreatePart<T>( this T @this, out IFunctionScopePart part, bool top = false ) where T : IFunctionScope
        {
            part = @this.CreatePart( top );
            return @this;
        }

        /// <summary>
        /// Creates a segment of code inside this namespace.
        /// This signature allows a fluent code to "emit" one or more insertion points.
        /// </summary>
        /// <typeparam name="T">The namespace scope type.</typeparam>
        /// <param name="this">This namespace scope.</param>
        /// <param name="part">The namespace part to use to inject code at this location (or at the top).</param>
        /// <param name="top">Optionally creates the new part at the start of the code instead of at the current writing position in the code.</param>
        /// <returns>This namespace scope writer to enable fluent syntax.</returns>
        public static T CreatePart<T>( this T @this, out INamespaceScopePart part, bool top = false ) where T : INamespaceScope
        {
            part = @this.CreatePart( top );
            return @this;
        }

        /// <summary>
        /// Creates a segment of code inside this type.
        /// This signature allows a fluent code to "emit" one or more insertion points.
        /// </summary>
        /// <typeparam name="T">The type scope type.</typeparam>
        /// <param name="this">This type scope.</param>
        /// <param name="part">The type part to use to inject code at this location (or at the top).</param>
        /// <param name="top">Optionally creates the new part at the start of the code instead of at the current writing position in the code.</param>
        /// <returns>This type scope writer to enable fluent syntax.</returns>
        public static T CreatePart<T>( this T @this, out ITypeScopePart part, bool top = false ) where T : ITypeScope
        {
            part = @this.CreatePart( top );
            return @this;
        }

        /// <summary>
        /// Fluent function application: this enables a procedural fragment to be inlined in a fluent code.
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="f">Fluent function to apply.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        public static T Append<T>( this T @this, Func<T, T> f ) where T : ICodeWriter => f( @this );

        /// <summary>
        /// Fluent action application: this enables a procedural fragment to be inlined in a fluent code.
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="f">Actio to apply to this code writer.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        public static T Append<T>( this T @this, Action<T> f ) where T : ICodeWriter
        {
            f( @this );
            return @this;
        }
    }
}
