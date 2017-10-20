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
        /// Appends raw C# code.
        /// This is the most basic Append method to use.
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T Append<T>( this T @this, string code ) where T : ICodeWriter
        {
            @this.DoAdd( code );
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
        /// When sets to true, typeof( Dictionary<,>.KeyCollection )
        /// will append "System.Collections.Generic.Dictionary&lt;TKey,TValue&gt;.KeyCollection".
        /// When false (the default), it will append "System.Collections.Generic.Dictionary<,>.KeyCollection".
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="t">The type to obtain.</param>
        /// <param name="typeDeclaration">True to include orginal generic parameters in the output.</param>
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
        /// Appends either "true" or "false".
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="b">The boolean value.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T Append<T>( this T @this, bool b ) where T : ICodeWriter => @this.Append( b ? "true" : "false" );

        /// <summary>
        /// Appends the code of a set of objetcs of a given type <typeparamref name="T"/>.
        /// The code is either "null", <see cref="Array.Empty{T}()"/> or an actual new array
        /// with the items appended with <see cref="Append(ICodeWriter, object)"/>: only
        /// basic types are supported.
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <typeparam name="TItem">The items type.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="e">Set of items for which code must be generated. Can be null.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T Append<T,TItem>( this T @this, IEnumerable<TItem> e ) where T : ICodeWriter
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
        /// Appends the source representation of a character: "'c'".
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="c">The character.</param>
        /// <param name="this">This code writer to enable fluent syntax.</param>
        static public T Append<T>( this T @this, char c ) where T : ICodeWriter
        {
            return c == '\\'
                    ? @this.Append( @"'\\'" )
                    : @this.Append( "'" ).Append( c.ToString() ).Append( "'" );
        }

        /// <summary>
        /// Appends the source representation of an integer value.
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="i">The integer.</param>
        /// <param name="this">This code writer to enable fluent syntax.</param>
        static public T Append<T>( this T @this, int i ) where T : ICodeWriter
        {
            return @this.Append( i.ToString( CultureInfo.InvariantCulture ) );
        }

        /// <summary>
        /// Appends the source representation of a long value (0 is appended as "0L").
        /// </summary>
        /// <param name="this">This code writer.</param>
        /// <param name="i">The long.</param>
        /// <param name="this">This code writer to enable fluent syntax.</param>
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
        /// <param name="this">This code writer to enable fluent syntax.</param>
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
        /// <param name="this">This code writer to enable fluent syntax.</param>
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
        /// <param name="this">This code writer to enable fluent syntax.</param>
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
        /// <param name="this">This code writer to enable fluent syntax.</param>
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
        /// <param name="this">This code writer to enable fluent syntax.</param>
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
        /// <param name="this">This code writer to enable fluent syntax.</param>
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
        /// <param name="this">This code writer to enable fluent syntax.</param>
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
        /// <param name="this">This code writer to enable fluent syntax.</param>
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
        /// <param name="this">This code writer to enable fluent syntax.</param>
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
        /// <param name="this">This code writer to enable fluent syntax.</param>
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
        /// <param name="this">This code writer to enable fluent syntax.</param>
        static public T Append<T>( this T @this, DateTime d ) where T : ICodeWriter
        {
            return @this.Append( "new DateTime(" ).Append( d.Ticks ).Append( ", DateTimeKind." ).Append( d.Kind.ToString() ).Append( ")" );
        }

        /// <summary>
        /// Appends the source representation of a <see cref="TimeSpan"/> value: "new TimeSpan( ... )".
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="d">The time span.</param>
        /// <param name="this">This code writer to enable fluent syntax.</param>
        static public T Append<T>( this T @this, TimeSpan ts ) where T : ICodeWriter
        {
            return @this.Append( "new TimeSpan(" ).Append( ts.Ticks.ToString( CultureInfo.InvariantCulture ) ).Append( ")" );
        }

        /// <summary>
        /// Appends the source representation of a <see cref="DateTimeOffset"/> value: "new DateTimeOffset( ... )".
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="d">The date time with offset.</param>
        /// <param name="this">This code writer to enable fluent syntax.</param>
        static public T Append<T>( this T @this, DateTimeOffset to ) where T : ICodeWriter
        {
            return @this.Append( "new DateTimeOffset(" )
                        .Append( to.Ticks )
                        .Append( ", new TimeSpan(" )
                        .Append( to.Offset.Ticks )
                        .Append( "))" );
        }

        /// <summary>
        /// Appends the code of a set of objects.
        /// If the actual set is a <see cref="IEnumerable{T}"/>, the actual type is extracted
        /// otherwise the type of the items is considered as being object.
        /// The code is either "null", <see cref="Array.Empty{T}()"/> or an actual new array
        /// with the items appended with <see cref="Append(ICodeWriter, object)"/>: only
        /// basic types are supported.
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="e">Set of items for which code must be generated. Can be null.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T Append<T>( this T @this, IEnumerable e ) where T : ICodeWriter
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
        /// Appends the source representation of the string.
        /// See <see cref="ExternalTypeExtensions.ToSourceString(string)"/>.
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="s">The string. Can be null.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T AppendString<T>( this T @this, string s ) where T : ICodeWriter
        {
            return @this.Append( s.ToSourceString() );
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
                case char x: return Append( @this, x );
                case double x: return Append( @this, x );
                case float x: return Append( @this, x );
                case Decimal x: return Append( @this, x );
                case DateTime x: return Append( @this, x );
                case TimeSpan x: return Append( @this, x );
                case DateTimeOffset x: return Append( @this, x );
                case IEnumerable x: return Append( @this, x );
            }
            throw new ArgumentException( "Unknown type: " + o.GetType().AssemblyQualifiedName );
        }
    }
}
