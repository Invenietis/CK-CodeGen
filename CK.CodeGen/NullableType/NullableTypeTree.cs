using CK.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CK.CodeGen
{
    /// <summary>
    /// Full immutable representation of a Nullable Type with its <see cref="SubTypes"/>.
    /// This does not capture nesting/enclosing generic types: this can be computed only for top level types or types nested in non generic types.
    /// </summary>
    public readonly struct NullableTypeTree
    {
        /// <summary>
        /// The type.
        /// When Type is a <see cref="NullablityTypeKindExtension.IsNullableValueType(NullabilityTypeKind)"/> (a <see cref="Nullable{T}"/>),
        /// then this type is the inner type, not the Nullable generic type.
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
        internal NullableTypeTree( Type t, NullabilityTypeKind k, IReadOnlyList<NullableTypeTree> s )
        {
            Debug.Assert( !t.IsGenericType || t.GetGenericTypeDefinition() != typeof( Nullable<> ) );
            Type = t;
            Kind = k;
            SubTypes = s;
        }

        /// <summary>
        /// Produces a string description of this <see cref="NullableTypeTree"/>.
        /// </summary>
        /// <param name="b">The string builder to use.</param>
        /// <param name="withNamespace">True to include the types' namespace and enclosing types (non generic) if any.</param>
        /// <returns>The string builder.</returns>
        public StringBuilder ToString( StringBuilder b, bool withNamespace = false )
        {
            if( Type.IsArray )
            {
                SubTypes[0].ToString( b, withNamespace );
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
                            if( withNamespace ) DumpNamespace( b, Type );
                        }
                        b.Append( n ).Append( '<' );
                    }
                    bool atLeastOne = false;
                    foreach( var t in SubTypes )
                    {
                        if( atLeastOne ) b.Append( ',' );
                        else atLeastOne = true;
                        t.ToString( b, withNamespace );
                    }
                    b.Append( isTuple ? ')' : '>' );
                }
                else
                {
                    var n = CodeWriterExtensions.GetTypeAlias( Type );
                    if( n == null ) 
                    {
                        if( withNamespace ) DumpNamespace( b, Type );
                        n = Type.Name;
                    }
                    b.Append( n );
                }
            }
            if( Kind.IsNullable() ) b.Append( '?' );
            return b;

            static void DumpNamespace( StringBuilder b, Type t, bool withName = false )
            {
                if( t.DeclaringType != null ) DumpNamespace( b, t.DeclaringType, true );
                else b.Append( t.Namespace ).Append( '.' );
                if( withName ) b.Append( t.Name ).Append( '.' );
            }

        }


        /// <summary>
        /// Produces a string description of this <see cref="NullableTypeTree"/>.
        /// </summary>
        /// <param name="withNamespace">True to include the types' namespace and enclosing types (non generic) if any.</param>
        /// <returns>The type name.</returns>
        public string ToString( bool withNamespace ) => ToString( new StringBuilder(), withNamespace ).ToString();

        /// <summary>
        /// Calls <see cref="ToString(StringBuilder,bool)"/> (without namespaces) and returns the result.
        /// </summary>
        /// <returns>The type name whithout namespace not nesting types.</returns>
        public override string ToString() => ToString( false );
    }
}

