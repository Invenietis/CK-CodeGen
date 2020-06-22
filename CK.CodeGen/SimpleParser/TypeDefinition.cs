using CK.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.CodeGen
{
    /// <summary>
    /// Type definition includes base types
    /// </summary>
    class TypeDefinition : IComparable<TypeDefinition>, IEquatable<TypeDefinition>
    {
        readonly int _hash;

        public enum TypeKind
        {
            None,
            Class,
            Interface,
            Enum,
            Struct
        }

        public IReadOnlyList<AttributeDefinition> Attributes { get; }

        public Modifiers Modifiers { get; }

        public TypeKind Kind { get; }

        public  TypeName Name { get; }

        public IReadOnlyList<TypeName> BaseTypes { get; }

        public IReadOnlyList<TypeParameterConstraint> Constraints { get; }

        internal TypeDefinition(
            IReadOnlyList<AttributeDefinition>? attributes,
            Modifiers modifiers,
            TypeKind kind,
            TypeName name,
            IReadOnlyList<TypeName>? bases,
            IReadOnlyList<TypeParameterConstraint>? constraints )
        {
            Attributes = attributes ?? Array.Empty<AttributeDefinition>();
            Modifiers = modifiers;
            Kind = kind;
            Name = name;
            BaseTypes = bases ?? Array.Empty<TypeName>();
            Constraints = constraints ?? Array.Empty<TypeParameterConstraint>();
            _hash = Modifiers.GetHashCode() ^ Kind.GetHashCode() ^ Name.GetHashCode();
            foreach( var a in Attributes ) _hash ^= a.GetHashCode();
            foreach( var b in BaseTypes ) _hash ^= b.GetHashCode();
            foreach( var c in Constraints ) _hash ^= c.GetHashCode();
        }

        public StringBuilder Write( StringBuilder b )
        {
            foreach( var a in Attributes ) a.Write( b );
            Modifiers.Write( b );
            switch(Kind)
            {
                case TypeKind.Class: b.Append( "class " ); break;
                case TypeKind.Interface: b.Append( "interface " ); break;
                case TypeKind.Struct: b.Append( "struct " ); break;
                case TypeKind.Enum: b.Append( "enum " ); break;
            }
            Name.Write( b );
            bool already = false;
            if( BaseTypes.Count > 0 )
            {
                b.Append( " : " );
                foreach( var t in BaseTypes )
                {
                    if( already ) b.Append( ", " );
                    else already = true;
                    t.Write( b );
                }
            }
            if( Constraints.Count > 0 )
            {
                b.Append( ' ' );
                already = false;
                foreach( var c in Constraints )
                {
                    if( already ) b.Append( ' ' );
                    else already = true;
                    c.Write( b );
                }
            }
            return b;
        }

        public int CompareTo( TypeDefinition other )
        {
            int cmp = Modifiers - other.Modifiers;
            if( cmp != 0 ) return cmp;
            cmp = StringComparer.Ordinal.Compare( Name, other.Name );
            if( cmp != 0 ) return cmp;
            cmp = Attributes.Count - other.Attributes.Count;
            if( cmp != 0 ) return cmp;
            for( int i = 0; i < Attributes.Count; ++i )
            {
                cmp = Attributes[i].CompareTo( other.Attributes[i] );
                if( cmp != 0 ) return cmp;
            }
            cmp = BaseTypes.Count - other.BaseTypes.Count;
            if( cmp != 0 ) return cmp;
            for( int i = 0; i < BaseTypes.Count; ++i )
            {
                cmp = BaseTypes[i].CompareTo( other.BaseTypes[i] );
                if( cmp != 0 ) return cmp;
            }
            cmp = Constraints.Count - other.Constraints.Count;
            if( cmp != 0 ) return cmp;
            for( int i = 0; i < Constraints.Count; ++i )
            {
                cmp = Constraints[i].CompareTo( other.Constraints[i] );
                if( cmp != 0 ) return cmp;
            }
            return cmp;
        }

        public bool Equals( TypeDefinition other )
        {
            return Modifiers == other.Modifiers
                    && Name.Equals( other.Name )
                    && Attributes.Count == other.Attributes.Count
                    && BaseTypes.Count == other.BaseTypes.Count
                    && Constraints.Count == other.Constraints.Count
                    && BaseTypes.SequenceEqual( other.BaseTypes )
                    && Constraints.SequenceEqual( Constraints )
                    && Attributes.SequenceEqual( Attributes );
        }

        public override bool Equals( object obj ) => obj is TypeDefinition o && Equals( o );

        public override int GetHashCode() => _hash;

        public override string ToString() => Write( new StringBuilder() ).ToString();


    }
}
