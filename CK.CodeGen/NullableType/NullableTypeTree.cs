using CK.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CK.CodeGen
{
    /// <summary>
    /// Full immutable representation of a Nullable Type with its <see cref="RawSubTypes"/>.
    /// This does not capture nesting/enclosing generic types: this can be computed only for top level types or types nested in non generic types.
    /// <para>
    /// The extension method <see cref="NullabilityTypeExtensions.GetNullableTypeTree(Type, NullabilityTypeInfo)"/> is the factory method to use
    /// to obtain this detailed information...
    /// </para>
    /// <para>
    /// ...or one of the other extension methods that first obtain the <see cref="NullabilityTypeInfo"/>:
    /// <see cref="NullabilityTypeExtensions.GetNullableTypeTree(System.Reflection.PropertyInfo)"/>, <see cref="NullabilityTypeExtensions.GetNullableTypeTree(System.Reflection.ParameterInfo)"/>
    /// or <see cref="NullabilityTypeExtensions.GetNullableTypeTree(System.Reflection.FieldInfo)"/>.
    /// </para>
    /// <para>
    /// When no Nullable Reference Type (NRT) context is available (oblivious context), the basic <see cref="NullabilityTypeExtensions.GetNullableTypeTree(Type)"/>
    /// can be used.
    /// </para>
    /// </summary>
    public readonly struct NullableTypeTree : IEquatable<NullableTypeTree>
    {
        /// <summary>
        /// The type.
        /// When this <see cref="Kind"/> is a <see cref="NullablityTypeKindExtension.IsNullableValueType(NullabilityTypeKind)"/> (a <see cref="Nullable{T}"/>),
        /// then this type is the inner type, not the Nullable generic type.
        /// </summary>
        public readonly Type Type;

        /// <summary>
        /// The subordinates types if any. Can be generic parameters of the <see cref="Type"/> or the item type of an array.
        /// This "raw" types are the direct children: for <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}"/> only
        /// the 8 types appear (including the last singleton value tuple).
        /// </summary>
        public readonly IReadOnlyList<NullableTypeTree> RawSubTypes;

        /// <summary>
        /// The subordinates types if any. This flattens <see cref="RawSubTypes"/> if <see cref="IsLongValueTuple"/> is true.
        /// </summary>
        public readonly IEnumerable<NullableTypeTree> SubTypes => IsLongValueTuple
                                                                    ? RawSubTypes.Take( 7 ).Concat( RawSubTypes[7].SubTypes )
                                                                    : RawSubTypes;

        /// <summary>
        /// The <see cref="NullabilityTypeKind"/> for this <see cref="Type"/>.
        /// </summary>
        public readonly NullabilityTypeKind Kind;

        /// <summary>
        /// Gets whether this is a <see cref="ValueTuple{T1, T2, T3, T4, T5, T6, T7, TRest}"/>: TRest is a singleton <see cref="ValueTuple{T}"/>:
        /// the actual value tuple contains 8 parameters or more.
        /// </summary>
        public bool IsLongValueTuple => Kind.IsTupleType() && RawSubTypes.Count == 8;

        /// <summary>
        /// Gets whether this top <see cref="Type"/> is a "normal null".
        /// We consider "Null Normality" as simply being <c>Kind.IsReferenceType() == Kind.IsNullable()</c>: a reference type
        /// is "normally nullable" and a value type is "normally not nullable".
        /// <para>
        /// This acts as a normalized form that can avoid doubling a cache of <see cref="NullableTypeTree"/> information when
        /// the cached information is easily computed to the opposite "not normally nullable" form (like adding or removing a '?'
        /// after a type name).
        /// </para>
        /// </summary>
        public bool IsNormalNull => Kind.IsReferenceType() == Kind.IsNullable();

        /// <summary>
        /// Returns this full <see cref="NullableTypeTree"/> with its top <see cref="Type"/> in <see cref="IsNormalNull"/> form:
        /// <list type="bullet">
        /// <item>
        /// If this is a nullable reference type or a non nullable value type, this is returned unchanged.
        /// </item>
        /// <item>
        /// If this is a non-nullable reference type, a new tree with the same <see cref="Type"/> and a <see cref="NullabilityTypeKind.IsNullable"/> kind
        /// is returned.
        /// </item>
        /// <item>
        /// If this is a nullable value type, a new tree with the (already not nullable) <see cref="Type"/> and a non <see cref="NullabilityTypeKind.IsNullable"/> kind
        /// is returned.
        /// </item>
        /// </list>
        /// This doesn't change the type tree in depth, only this top Type/Kind is concerned by this normalization.
        /// </summary>
        /// <returns>This or a normalized tree.</returns>
        public NullableTypeTree ToNormalNull()
        {
            if( Kind.IsReferenceType() )
            {
                return Kind.IsNullable()
                        ? this
                        : new NullableTypeTree( Type, Kind | NullabilityTypeKind.IsNullable, RawSubTypes );
            }
            return Kind.IsNullable()
                    ? new NullableTypeTree( Type, Kind & ~(NullabilityTypeKind.IsNullable|NullabilityTypeKind.IsTechnicallyNullable), RawSubTypes )
                    : this;
        }

        /// <summary>
        /// Initializes a new <see cref="NullableTypeTree"/>.
        /// </summary>
        /// <param name="t">The Type.</param>
        /// <param name="k">The <see cref="NullabilityTypeKind"/>.</param>
        /// <param name="s">The sub types (generic parameters or array element).</param>
        internal NullableTypeTree( Type t, NullabilityTypeKind k, IReadOnlyList<NullableTypeTree> s )
        {
            Debug.Assert( !t.IsGenericType || t.GetGenericTypeDefinition() != typeof( Nullable<> ) );
            Type = t;
            Kind = k;
            RawSubTypes = s;
        }

        /// <summary>
        /// See <see cref="Equals(NullableTypeTree)"/>.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True if they are the same, false otherwise.</returns>
        public override bool Equals( object? obj ) => obj is NullableTypeTree tree && Equals( tree );

        /// <summary>
        /// Implements a strict equality except that <see cref="NullabilityTypeKind.NRTFullNullable"/> and <see cref="NullabilityTypeKind.NRTFullNonNullable"/> bits
        /// are ignored.
        /// </summary>
        /// <param name="other">The other nullable type tree.</param>
        /// <returns>True if they are the same, false otherwise.</returns>
        public bool Equals( NullableTypeTree other )
        {
            return (Kind & ~(NullabilityTypeKind.NRTFullNonNullable | NullabilityTypeKind.NRTFullNullable)) == (other.Kind & ~(NullabilityTypeKind.NRTFullNonNullable | NullabilityTypeKind.NRTFullNullable))
                    && EqualityComparer<Type>.Default.Equals( Type, other.Type )
                    && RawSubTypes.SequenceEqual( other.RawSubTypes );
        }

        /// <summary>
        /// See <see cref="Equals(NullableTypeTree)"/>.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            HashCode c = new HashCode();
            c.Add( Type );
            c.Add( (byte)(Kind & ~(NullabilityTypeKind.NRTFullNonNullable | NullabilityTypeKind.NRTFullNullable)) );
            foreach( var t in RawSubTypes ) c.Add( t.GetHashCode() );
            return c.ToHashCode();
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
                RawSubTypes[0].ToString( b, withNamespace );
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
        /// <returns>The type name without namespace nor nesting types.</returns>
        public override string ToString() => ToString( false );
    }
}

