using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.CodeGen
{
    /// <summary>
    /// Immutable type name that can be a single indentifier ("T" or "int"), a generic definition
    /// or generic type ("C&lt;T&gt;" or S&lt;&lt;string&gt;,List&lt;int&gt;&gt;) with a list of <see cref="GenericParameters"/>
    /// and an array definition (jagged, and/or multi-dimensional array) of such types if <see cref="ArrayDimensions"/> is not empty.
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
        public readonly struct GenParam
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
                /// Indicates convariance.
                /// </summary>
                Out
            }

            /// <summary>
            /// The optional variance.
            /// </summary>
            public readonly Variance TypeVariance;

            /// <summary>
            /// The type name. May be a generic argument or an actual type.
            /// </summary>
            public readonly ExtendedTypeName Type;

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
        }

        readonly string _name;
        readonly IReadOnlyList<GenParam> _genArgs;
        readonly IReadOnlyList<int> _arrayDims;

        TypeName()
        {
            _name = String.Empty;
            _genArgs = Array.Empty<GenParam>();
            _arrayDims = Array.Empty<int>();
            TypeDefinitionKey = String.Empty;
        }

        /// <summary>
        /// Initializes a new <see cref="TypeName"/>.
        /// When used directly, the <paramref name="name"/> can be a complex one and is not analyzed.
        /// </summary>
        /// <param name="name">The type name is an identifier or a full type name.</param>
        /// <param name="genericParameters">An optional list of <see cref="GenParam"/>.</param>
        /// <param name="arrayDimensions">An optional list of array dimensions. See <see cref="ArrayDimensions"/>.</param>
        public TypeName( string name, IReadOnlyList<GenParam>? genericParameters = null, IReadOnlyList<int>? arrayDimensions = null )
        {
            if( String.IsNullOrWhiteSpace( name ) ) throw new ArgumentOutOfRangeException( nameof( name ) );
            _name = name;
            _genArgs = genericParameters ?? Array.Empty<GenParam>();
            _arrayDims = arrayDimensions ?? Array.Empty<int>();
            TypeDefinitionKey = _name;
            if( _genArgs.Count > 0 ) TypeDefinitionKey += "`" + _genArgs.Count;
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
        public string TypeDefinitionKey { get; }

        /// <summary>
        /// Gets the number of arrays (this defines a jagged array when there is two or more
        /// numbers in this list) and for each of them, its "dimension" (the number of ',' commas
        /// inside): 0 for a standard one-dimensional array, 1 for a two-dimensional one, etc.
        /// </summary>
        public IReadOnlyList<int> ArrayDimensions => _arrayDims;

        /// <summary>
        /// Writes this TypeName into the provided StringBuilder.
        /// </summary>
        /// <param name="b">The target.</param>
        /// <returns>The StringBuilder to enable fluent syntax.</returns>
        public StringBuilder Write( StringBuilder b )
        {
            b.Append( _name );
            WriteGenericParameters( b );
            foreach( int d in _arrayDims )
            {
                b.Append( '[' ).Append( ',', d ).Append( ']' );
            }
            return b;
        }

        /// <summary>
        /// Writes the <see cref="GenericParameters"/> into the provided StringBuilder.
        /// </summary>
        /// <param name="b">The target.</param>
        /// <returns>The StringBuilder to enable fluent syntax.</returns>
        public StringBuilder WriteGenericParameters( StringBuilder b )
        {
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
                    g.Type.Write( b );
                }
                b.Append( '>' );
            }
            return b;
        }

        /// <summary>
        /// Overridden to return the <see cref="Write(StringBuilder)"/> result.
        /// </summary>
        /// <returns>The type string.</returns>
        public override string ToString() => Write( new StringBuilder() ).ToString();

    }

}
