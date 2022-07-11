using CK.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.CodeGen
{
    /// <summary>
    /// Immutable type name that can be a single identifier ("T" or "int"), a generic definition
    /// or generic type ("C&lt;T&gt;" or S&lt;&lt;string&gt;,List&lt;int&gt;&gt;) with a list of <see cref="GenericParameters"/>.
    /// Arrays and tuples are handled at the <see cref="ExtendedTypeName"/> level.
    /// </summary>
    public class TypeName
    {
        /// <summary>
        /// Gets an empty type name instance.
        /// </summary>
        public static readonly TypeName Empty = new TypeName();

        /// <summary>
        /// Captures a generic argument (with its optional <see cref="TypeVariance"/>) or a
        /// generic type parameter.
        /// </summary>
        public readonly struct GenParam : IEquatable<GenParam>
        {
            /// <summary>
            /// Gets an empty generic parameter instance.
            /// </summary>
            public static readonly GenParam Empty = new TypeName.GenParam( Variance.None, ExtendedTypeName.Empty );

            /// <summary>
            /// Optional "out" or "in" modifier for generic arguments.
            /// </summary>
            public enum Variance
            {
                /// <summary>
                /// No variance.
                /// </summary>
                None,

                /// <summary>
                /// Indicates contravariance.
                /// </summary>
                In,

                /// <summary>
                /// Indicates covariance.
                /// </summary>
                Out
            }

            /// <summary>
            /// The optional variance.
            /// </summary>
            public Variance TypeVariance { get; }

            /// <summary>
            /// The type name. May be a generic argument or an actual type.
            /// </summary>
            public ExtendedTypeName Type { get; }

            /// <summary>
            /// Initializes a new generic parameter.
            /// </summary>
            /// <param name="v">The variance.</param>
            /// <param name="t">The type.</param>
            public GenParam( Variance v, ExtendedTypeName t )
            {
                TypeVariance = v;
                Type = t;
            }

            /// <summary>
            /// Overridden to return the variance and the type name.
            /// </summary>
            /// <returns>A readable string.</returns>
            public override string ToString() => TypeVariance switch
            {
                Variance.In => "in " + Type.ToString(),
                Variance.Out => "out " + Type.ToString(),
                _ => Type.ToString()
            };

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
            public override bool Equals( object? obj ) => obj is GenParam o ? Equals( o ) : false;

            public override int GetHashCode() => Type.GetHashCode();

            public bool Equals( GenParam other ) => TypeVariance == other.TypeVariance && Type == other.Type;

            public static bool operator ==( GenParam left, GenParam right ) => left.Equals( right );

            public static bool operator !=( GenParam left, GenParam right ) => !(left == right);
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        }

        readonly string _name;
        readonly IReadOnlyList<GenParam> _genArgs;

        TypeName()
        {
            _name = String.Empty;
            _genArgs = Array.Empty<GenParam>();
            Key = String.Empty;
        }

        /// <summary>
        /// Initializes a new <see cref="TypeName"/>.
        /// When used directly, the <paramref name="name"/> can be a complex one and is not analyzed.
        /// </summary>
        /// <param name="name">The type name is an identifier or a full type name.</param>
        /// <param name="genericParameters">An optional list of <see cref="GenParam"/>.</param>
        public TypeName( string name, IReadOnlyList<GenParam>? genericParameters = null )
        {
            if( String.IsNullOrWhiteSpace( name ) ) throw new ArgumentOutOfRangeException( nameof( name ) );
            _name = name;
            _genArgs = genericParameters ?? Array.Empty<GenParam>();
            Key = _name;
            if( _genArgs.Count > 0 ) Key += "`" + _genArgs.Count;
        }

        /// <summary>
        /// Gets the type name identifier.
        /// </summary>
        public string Name => _name;

        /// <summary>
        /// Gets the generic parameters.
        /// </summary>
        public IReadOnlyList<GenParam> GenericParameters => _genArgs;

        /// <summary>
        /// Gets the normalized key that identifies this type as a definition relatively to
        /// a namespace: it is "<see cref="Name"/>" alone for non generic types and
        /// "<see cref="Name"/>`<see cref="GenericParameters"/>.Count" for generic ones.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Writes this TypeName into the provided StringBuilder.
        /// </summary>
        /// <param name="b">The target.</param>
        /// <param name="typeNameReplacer">Optional name replacer function.</param>
        /// <returns>The StringBuilder to enable fluent syntax.</returns>
        public StringBuilder Write( StringBuilder b, Func<string,string>? typeNameReplacer = null )
        {
            Throw.CheckNotNullArgument( b );
            b.Append( typeNameReplacer != null ? typeNameReplacer( _name ) : _name );
            WriteGenericParameters( b, typeNameReplacer );
            return b;
        }

        /// <summary>
        /// Writes the <see cref="GenericParameters"/> into the provided StringBuilder.
        /// </summary>
        /// <param name="b">The target.</param>
        /// <param name="typeNameReplacer">Optional naked type name replacer function.</param>
        /// <returns>The StringBuilder to enable fluent syntax.</returns>
        public StringBuilder WriteGenericParameters( StringBuilder b, Func<string, string>? typeNameReplacer = null )
        {
            Throw.CheckNotNullArgument( b );
            if( _genArgs.Count > 0 )
            {
                b.Append( '<' );
                bool already = false;
                foreach( var g in _genArgs )
                {
                    if( already ) b.Append( ',' );
                    else already = true;
                    if( g.TypeVariance == GenParam.Variance.In ) b.Append( "in " );
                    else if( g.TypeVariance == GenParam.Variance.Out ) b.Append( "out " );
                    g.Type.Write( b, typeNameReplacer );
                }
                b.Append( '>' );
            }
            return b;
        }

        /// <summary>
        /// Overridden to return the <see cref="Write"/> result.
        /// </summary>
        /// <returns>The type string.</returns>
        public override string ToString() => Write( new StringBuilder() ).ToString();

    }

}
