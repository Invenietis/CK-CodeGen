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
        static public T RawAppend<T>( this T @this, string code ) where T : ICodeWriter
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
        static public T AppendWhiteSpace<T>( this T @this ) where T : ICodeWriter => @this.RawAppend( " " );

        /// <summary>
        /// Appends a <see cref="Environment.NewLine"/>.
        /// </summary>
        /// <typeparam name="T">Actual type of the code writer.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T AppendLine<T>( this T @this ) where T : ICodeWriter => @this.RawAppend( Environment.NewLine );

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
            if( t == null ) return @this.RawAppend( "null" );
            if( t.IsGenericParameter ) return typeDeclaration ? @this.RawAppend( t.Name ) : @this;
            string alias;
            if( _typeAliases.TryGetValue( t, out alias ) )
            {
                return @this.RawAppend( alias );
            }
            if( t == typeof( void ) ) return @this.RawAppend( "void" );
            var pathTypes = new Stack<Type>();
            pathTypes.Push( t );
            Type decl = t.DeclaringType;
            while( decl != null )
            {
                pathTypes.Push( decl );
                decl = decl.DeclaringType;
            }
            //var tInfo = t.GetType();
            var allGenArgs = new Queue<Type>( t.GetGenericArguments() );
            //var allGenArgs = new Queue<Type>( tInfo.GenericTypeArguments );
            //foreach( var p in tInfo.GetGenericArguments() ) allGenArgs.Enqueue( p );
            for( int iType = 0; pathTypes.Count > 0; iType++ )
            {
                Type theT = pathTypes.Pop();
                //TypeInfo theTInfo = theT.GetTypeInfo();
                string n;
                if( iType == 0 ) n = theT.FullName;
                else
                {
                    n = theT.Name;
                    @this.RawAppend( "." );
                }
                int idxTick = n.IndexOf( '`' ) + 1;
                if( idxTick > 0 )
                {
                    int endNbParam = idxTick;
                    while( endNbParam < n.Length && Char.IsDigit( n, endNbParam ) ) endNbParam++;
                    int nbParams = int.Parse( n.Substring( idxTick, endNbParam - idxTick ), NumberStyles.Integer );
                    Debug.Assert( nbParams > 0 );
                    @this.RawAppend( n.Substring( 0, idxTick - 1 ) );
                    @this.RawAppend( "<" );
                    for( int iGen = 0; iGen < nbParams; ++iGen )
                    {
                        if( iGen > 0 ) @this.RawAppend( "," );
                        AppendCSharpName( @this, allGenArgs.Dequeue(), typeDeclaration );
                    }
                    @this.RawAppend( ">" );
                }
                else @this.RawAppend( n );
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
        static public T Append<T>( this T @this, bool b ) where T : ICodeWriter => @this.RawAppend( b ? "true" : "false" );

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
            if( e == null ) return @this.RawAppend( "null" );
            if( !e.Any() ) return @this.RawAppend( "Array.Empty<" ).AppendCSharpName( typeof( TItem ), false ).RawAppend( ">()" );
            @this.RawAppend( "new " ).AppendCSharpName( typeof( TItem ), false ).RawAppend( "[]{" );
            bool already = false;
            foreach( TItem x in e )
            {
                if( already ) @this.RawAppend( "," );
                else already = true;
                Append( @this, x );
            }
            return @this.RawAppend( "}" );
        }

        /// <summary>
        /// Appends the source representation of a character: "'c'".
        /// </summary>
        /// <param name="this">This code writer.</param>
        /// <param name="c">The character.</param>
        /// <param name="this">This code writer to enable fluent syntax.</param>
        static public T Append<T>( this T @this, char c ) where T : ICodeWriter
        {
            return c == '\\'
                    ? @this.RawAppend( @"'\\'" )
                    : @this.RawAppend( "'" ).RawAppend( c.ToString() ).RawAppend( "'" );
        }

        /// <summary>
        /// Appends the source representation of an integer value.
        /// </summary>
        /// <param name="this">This code writer.</param>
        /// <param name="i">The integer.</param>
        /// <param name="this">This code writer to enable fluent syntax.</param>
        static public T Append<T>( this T @this, int i ) where T : ICodeWriter
        {
            return @this.RawAppend( i.ToString( CultureInfo.InvariantCulture ) );
        }

        /// <summary>
        /// Appends the source representation of a long value (0 is appended as "0L").
        /// </summary>
        /// <param name="this">This code writer.</param>
        /// <param name="i">The long.</param>
        /// <param name="this">This code writer to enable fluent syntax.</param>
        static public T Append<T>( this T @this, long i ) where T : ICodeWriter
        {
            return @this.RawAppend( i.ToString( CultureInfo.InvariantCulture ) ).RawAppend( "L" );
        }

        /// <summary>
        /// Appends the source representation of a <see cref="UInt64"/> value (0 is appended as "(ulong)0").
        /// </summary>
        /// <param name="this">This code writer.</param>
        /// <param name="i">The unsigned long.</param>
        /// <param name="this">This code writer to enable fluent syntax.</param>
        static public T Append<T>( this T @this, ulong i ) where T : ICodeWriter
        {
            return @this.RawAppend( "(ulong)" ).RawAppend( i.ToString( CultureInfo.InvariantCulture ) );
        }

        /// <summary>
        /// Appends the source representation of a <see cref="Int16"/> value (0 is appended as "(short)0").
        /// </summary>
        /// <param name="this">This code writer.</param>
        /// <param name="i">The short integer..</param>
        /// <param name="this">This code writer to enable fluent syntax.</param>
        static public T Append<T>( this T @this, short i ) where T : ICodeWriter
        {
            return @this.RawAppend( "(short)" ).RawAppend( i.ToString( CultureInfo.InvariantCulture ) );
        }

        /// <summary>
        /// Appends the source representation of a <see cref="UInt16"/> value (0 is appended as "(ushort)0").
        /// </summary>
        /// <param name="this">This code writer.</param>
        /// <param name="i">The unsigned short integer..</param>
        /// <param name="this">This code writer to enable fluent syntax.</param>
        static public T Append<T>( this T @this, ushort i ) where T : ICodeWriter
        {
            return @this.RawAppend( "(ushort)" ).RawAppend( i.ToString( CultureInfo.InvariantCulture ) );
        }

        /// <summary>
        /// Appends the source representation of a <see cref="SByte"/> value (0 is appended as "(sbyte)0").
        /// </summary>
        /// <param name="this">This code writer.</param>
        /// <param name="i">The signed byte.</param>
        /// <param name="this">This code writer to enable fluent syntax.</param>
        static public T Append<T>( this T @this, sbyte i ) where T : ICodeWriter
        {
            return @this.RawAppend( "(sbyte)" ).RawAppend( i.ToString( CultureInfo.InvariantCulture ) );
        }
        /// <summary>
        /// Appends the source representation of a <see cref="Byte"/> value (0 is appended as "(byte)0").
        /// </summary>
        /// <param name="this">This code writer.</param>
        /// <param name="i">The byte.</param>
        /// <param name="this">This code writer to enable fluent syntax.</param>
        static public T Append<T>( this T @this, byte i ) where T : ICodeWriter
        {
            return @this.RawAppend( "(byte)" ).RawAppend( i.ToString( CultureInfo.InvariantCulture ) );
        }

        /// <summary>
        /// Appends the source representation of a <see cref="UInt32"/> value (0 is appended as "(uint)0").
        /// </summary>
        /// <param name="this">This code writer.</param>
        /// <param name="i">The unsigned integer.</param>
        /// <param name="this">This code writer to enable fluent syntax.</param>
        static public T Append<T>( this T @this, uint i ) where T : ICodeWriter
        {
            return @this.RawAppend( "(uint)" ).RawAppend( i.ToString( CultureInfo.InvariantCulture ) );
        }

        /// <summary>
        /// Appends the source representation of a <see cref="Guid"/> value: "new Guid(...)".
        /// </summary>
        /// <param name="this">This code writer.</param>
        /// <param name="g">The Guid.</param>
        /// <param name="this">This code writer to enable fluent syntax.</param>
        static public T Append<T>( this T @this, Guid g ) where T : ICodeWriter
        {
            return @this.RawAppend( "new Guid(\"" ).RawAppend( g.ToString() ).RawAppend( "\")" );
        }

        /// <summary>
        /// Appends the source representation of a <see cref="Double"/> value.
        /// </summary>
        /// <param name="this">This code writer.</param>
        /// <param name="d">The double.</param>
        /// <param name="this">This code writer to enable fluent syntax.</param>
        static public T Append<T>( this T @this, double d ) where T : ICodeWriter
        {
            return @this.RawAppend( d.ToString( CultureInfo.InvariantCulture ) );
        }

        /// <summary>
        /// Appends the source representation of a <see cref="Single"/> value.
        /// </summary>
        /// <param name="this">This code writer.</param>
        /// <param name="f">The float.</param>
        /// <param name="this">This code writer to enable fluent syntax.</param>
        static public T Append<T>( this T @this, float f ) where T : ICodeWriter
        {
            return @this.RawAppend( f.ToString( CultureInfo.InvariantCulture ) ).RawAppend( "f" );
        }

        /// <summary>
        /// Appends the source representation of a <see cref="Decimal"/> value (0 is appended as "0m").
        /// </summary>
        /// <param name="this">This code writer.</param>
        /// <param name="d">The decimal.</param>
        /// <param name="this">This code writer to enable fluent syntax.</param>
        static public T Append<T>( this T @this, decimal d ) where T : ICodeWriter
        {
            return @this.RawAppend( d.ToString( CultureInfo.InvariantCulture ) ).RawAppend( "m" );
        }

        /// <summary>
        /// Appends the source representation of a <see cref="DateTime"/> value: "new DateTime( ... )".
        /// </summary>
        /// <param name="this">This code writer.</param>
        /// <param name="d">The datetime.</param>
        /// <param name="this">This code writer to enable fluent syntax.</param>
        static public T Append<T>( this T @this, DateTime d ) where T : ICodeWriter
        {
            return @this.RawAppend( "new DateTime(" ).Append( d.Ticks ).RawAppend( ", DateTimeKind." ).RawAppend( d.Kind.ToString() ).RawAppend( ")" );
        }

        /// <summary>
        /// Appends the source representation of a <see cref="TimeSpan"/> value: "new TimeSpan( ... )".
        /// </summary>
        /// <param name="this">This code writer.</param>
        /// <param name="d">The time span.</param>
        /// <param name="this">This code writer to enable fluent syntax.</param>
        static public T Append<T>( this T @this, TimeSpan ts ) where T : ICodeWriter
        {
            return @this.RawAppend( "new TimeSpan(" ).RawAppend( ts.Ticks.ToString( CultureInfo.InvariantCulture ) ).RawAppend( ")" );
        }

        /// <summary>
        /// Appends the source representation of a <see cref="DateTimeOffset"/> value: "new DateTimeOffset( ... )".
        /// </summary>
        /// <param name="this">This code writer.</param>
        /// <param name="d">The date time with offset.</param>
        /// <param name="this">This code writer to enable fluent syntax.</param>
        static public T Append<T>( this T @this, DateTimeOffset to ) where T : ICodeWriter
        {
            return @this.RawAppend( "new DateTimeOffset(" )
                        .Append( to.Ticks )
                        .RawAppend( ", new TimeSpan(" )
                        .Append( to.Offset.Ticks )
                        .RawAppend( "))" );
        }

        /// <summary>
        /// Appends the code of a set of objects.
        /// If the actual set is a <see cref="IEnumerable{T}"/>, the actual type is extracted
        /// otherwise the type of the items is considered as being object.
        /// The code is either "null", <see cref="Array.Empty{T}()"/> or an actual new array
        /// with the items appended with <see cref="Append(ICodeWriter, object)"/>: only
        /// basic types are supported.
        /// </summary>
        /// <typeparam name="T">The items type.</typeparam>
        /// <param name="this">This code writer.</param>
        /// <param name="e">Set of items for which code must be generated. Can be null.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T Append<T>( this T @this, IEnumerable e ) where T : ICodeWriter
        {
            if( e == null ) return @this.RawAppend( "null" );
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
                @this.RawAppend( "new " ).AppendCSharpName( type, false ).RawAppend( "[]{" );
                bool existing = false;
                foreach( var x in e )
                {
                    if( existing ) @this.RawAppend( "," );
                    else existing = true;
                    Append( @this, x );
                }
                return @this.RawAppend( "}" );
            }
            return @this.RawAppend( "Array.Empty<" ).AppendCSharpName( type, false ).RawAppend( ">()" );
        }

        /// <summary>
        /// Appends the source representation of the string.
        /// See <see cref="ExternalTypeExtensions.ToSourceString(string)"/>.
        /// </summary>
        /// <param name="this">This code writer.</param>
        /// <param name="s">The string. Can be null.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T Append<T>( this T @this, string s ) where T : ICodeWriter
        {
            return @this.RawAppend( s.ToSourceString() );
        }

        /// <summary>
        /// Appends the code source for an untyped object.
        /// Only types that are implemented throug one of the existing Append extension methods
        /// are supported: an <see cref="ArgumentException"/> is thrown for unsuported type.
        /// </summary>
        /// <param name="this">This code writer.</param>
        /// <param name="o">The object. Can be null.</param>
        /// <returns>This code writer to enable fluent syntax.</returns>
        static public T Append<T>( this T @this, object o ) where T : ICodeWriter
        {
            if( o == null ) return @this.RawAppend( "null" );
            if( o == System.Type.Missing ) return @this.RawAppend( "System.Type.Missing" );
            TypeInfo oT = o.GetType().GetTypeInfo();
            if( oT.IsValueType )
            {
                if( o is bool ) return Append( @this, (bool)o );
                if( o is int ) return Append( @this, (int)o );
                if( o is long ) return Append( @this, (long)o );
                if( o is short ) return Append( @this, (short)o );
                if( o is ushort ) return Append( @this, (ushort)o );
                if( o is sbyte ) return Append( @this, (sbyte)o );
                if( o is uint ) return Append( @this, (uint)o );
                if( o is ulong ) return Append( @this, (ulong)o );
                if( o is byte ) return Append( @this, (byte)o );
                if( o is Guid ) return Append( @this, (Guid)o );
                if( o is char ) return Append( @this, (char)o );
                if( o is double ) return Append( @this, (double)o );
                if( o is float ) return Append( @this, (float)o );
                if( o is decimal ) return Append( @this, (decimal)o );
                if( o is DateTime ) return Append( @this, (DateTime)o );
                if( o is TimeSpan ) return Append( @this, (TimeSpan)o );
                if( o is DateTimeOffset ) return Append( @this, (DateTimeOffset)o );
            }
            else
            {
                string s = o as string;
                if( s != null ) return @this.RawAppend( s.ToSourceString() );
                Type t = o as Type;
                if( t != null ) return @this.RawAppend( "typeof(" ).AppendCSharpName( t, false ).RawAppend( ")" );
                IEnumerable e = o as IEnumerable;
                if( e != null ) return Append( @this, e );
            }
            throw new ArgumentException( "Unknown type: " + oT.AssemblyQualifiedName );
        }
    }
}
