using CK.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CK.CodeGen
{
    class AttributeDefinition : IComparable<AttributeDefinition>, IEquatable<AttributeDefinition>
    {
        readonly int _hash;

        public class OneAttribute : IComparable<OneAttribute>, IEquatable<OneAttribute>
        {
            readonly int _hash;

            internal OneAttribute( TypeName name, IReadOnlyList<string> values )
            {
                Name = name;
                Values = values ?? Array.Empty<string>();
                _hash = Name.GetHashCode();
                foreach( var v in Values ) _hash ^= v.GetHashCode();
            }

            public TypeName Name { get; }

            public IReadOnlyList<string> Values { get; }

            public int CompareTo( OneAttribute other )
            {
                int cmp = Name.CompareTo( other.Name );
                if( cmp != 0 ) return cmp;
                cmp = Values.Count - other.Values.Count;
                if( cmp != 0 ) return cmp;
                for( int i = 0; i < Values.Count; ++i )
                {
                    cmp = StringComparer.Ordinal.Compare( Values[i], other.Values[i] );
                    if( cmp != 0 ) return cmp;
                }
                return cmp;
            }

            public bool Equals( OneAttribute other )
            {
                return Name == other.Name
                        && Values.Count == other.Values.Count
                        && Values.SequenceEqual( other.Values );
            }

            public override bool Equals( object obj ) => obj is OneAttribute o && Equals( o );

            public override int GetHashCode() => _hash;

            public StringBuilder Write( StringBuilder b )
            {
                Name.Write( b );
                if( Values.Count > 0 )
                {
                    b.Append( '(' ).AppendStrings( Values, "," ).Append( ')' );
                }
                return b;
            }
        }

        public AttributeDefinition( string? target, IReadOnlyList<OneAttribute> attributes )
        {
            Target = target ?? String.Empty;
            Attributes = attributes ?? Array.Empty<OneAttribute>();
            _hash = Target.GetHashCode();
            foreach( var a in Attributes ) _hash ^= a.GetHashCode();
        }

        public string Target { get; }

        public IReadOnlyList<OneAttribute> Attributes { get; }

        public StringBuilder Write( StringBuilder b )
        {
            b.Append( '[' );
            if( Target.Length > 0 ) b.Append( Target ).Append( ": " );
            bool already = false;
            foreach( var one in Attributes )
            {
                if( already ) b.Append( ", " );
                else already = true;
                one.Write( b );
            }
            b.Append( ']' );
            return b;
        }

        public int CompareTo( AttributeDefinition other )
        {
            int cmp = StringComparer.Ordinal.Compare( Target, other.Target );
            cmp = Attributes.Count - other.Attributes.Count;
            if( cmp != 0 ) return cmp;
            for( int i = 0; i < Attributes.Count; ++i )
            {
                cmp = StringComparer.Ordinal.Compare( Attributes[i], other.Attributes[i] );
                if( cmp != 0 ) return cmp;
            }
            return cmp;
        }

        public bool Equals( AttributeDefinition other )
        {
            return Target == other.Target
                    && Attributes.Count == other.Attributes.Count
                    && Attributes.SequenceEqual( other.Attributes );
        }

        public override bool Equals( object obj ) => obj is AttributeDefinition o && Equals( o );

        public override int GetHashCode() => _hash;

        internal AttributeDefinition Merge( AttributeDefinition a )
        {
            Debug.Assert( a.Target == Target );
            var attrs = Attributes.Concat( a.Attributes ).OrderBy( x => x ).ToList();
            attrs.Sort();
            return new AttributeDefinition( Target, attrs );
        }
    }
}
