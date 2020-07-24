using CK.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.CodeGen
{
    /// <summary>
    /// Full immutable representation of a Nullable Type with its <see cref="SubTypes"/>.
    /// </summary>
    public readonly struct NullableTypeTree
    {
        /// <summary>
        /// The type.
        /// </summary>
        public readonly Type Type;

        /// <summary>
        /// The <see cref="NullabilityTypeKind"/> for this <see cref="Type"/>.
        /// </summary>
        public readonly NullabilityTypeKind Kind;

        /// <summary>
        /// The subordinates types if any. Can be generic pararameters of the <see cref="Type"/> or the item type of an array.
        /// </summary>
        public readonly IReadOnlyList<NullableTypeTree> SubTypes;

        /// <summary>
        /// Initalizes a new <see cref="NullableTypeTree"/>.
        /// </summary>
        /// <param name="t">The Type.</param>
        /// <param name="k">The <see cref="NullabilityTypeKind"/>.</param>
        /// <param name="s">The sub types (generic parameters or array element).</param>
        public NullableTypeTree( Type t, NullabilityTypeKind k, IReadOnlyList<NullableTypeTree> s )
        {
            Type = t;
            Kind = k;
            SubTypes = s;
        }

        /// <summary>
        /// Produces a quick string description of this <see cref="NullabilityTypeKind"/>
        /// with simple type names (no name space nor enclosing types).
        /// </summary>
        /// <param name="b">The string builder to use.</param>
        /// <returns>The string builder.</returns>
        public StringBuilder ToString( StringBuilder b )
        {
            if( Type.IsArray )
            {
                SubTypes[0].ToString( b );
                b.Append( '[' ).Append( ',', Type.GetArrayRank() - 1 ).Append( ']' );
            }
            else
            {
                if( (Kind & NullabilityTypeKind.IsGenericType) != 0 )
                {
                    bool isTuple = Type.IsValueTuple();
                    if( isTuple )
                    {
                        b.Append( '(' );
                    }
                    else
                    {
                        var n = CodeWriterExtensions.GetTypeAlias( Type );
                        if( n == null )
                        {
                            n = Type.Name;
                            int idx = n.IndexOf( '`' );
                            if( idx > 0 ) n = n.Substring( 0, idx );
                        }
                        b.Append( n ).Append( '<' );
                    }
                    bool atLeastOne = false;
                    foreach( var t in SubTypes )
                    {
                        if( atLeastOne ) b.Append( ',' );
                        else atLeastOne = true;
                        t.ToString( b );
                    }
                    b.Append( isTuple ? ')' : '>' );
                }
                else
                {
                    b.Append( CodeWriterExtensions.GetTypeAlias( Type ) ?? Type.Name );
                }
            }
            if( Kind.IsNullable() ) b.Append( '?' );
            return b;
        }

        /// <summary>
        /// Calls <see cref="ToString(StringBuilder)"/> and returns the result.
        /// </summary>
        /// <returns>A readable string.</returns>
        public override string ToString() => ToString( new StringBuilder() ).ToString();
    }
}

