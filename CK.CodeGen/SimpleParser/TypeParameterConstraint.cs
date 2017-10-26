using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.CodeGen
{
    class TypeParameterConstraint : IComparable<TypeParameterConstraint>, IEquatable<TypeParameterConstraint>
    {
        readonly int _hash;

        public string ParameterName { get; }

        public IReadOnlyList<TypeName> Constraints { get; }

        internal TypeParameterConstraint( string name, IReadOnlyList<TypeName> constraints )
        {
            ParameterName = name;
            Constraints = constraints ?? Array.Empty<TypeName>();
            _hash = ParameterName.GetHashCode();
            foreach( var c in Constraints ) _hash = _hash ^ c.GetHashCode();
        }

        public StringBuilder Write( StringBuilder b )
        {
            b.Append( "where " ).Append( ParameterName ).Append( " : " );
            bool already = false;
            foreach( var t in Constraints )
            {
                if( already ) b.Append( ", " );
                else already = true;
                t.Write( b );
            }
            return b;
        }

        public int CompareTo( TypeParameterConstraint other )
        {
            int cmp = StringComparer.Ordinal.Compare( ParameterName, other.ParameterName );
            if( cmp != 0 ) return cmp;
            cmp = Constraints.Count - other.Constraints.Count;
            if( cmp != 0 ) return cmp;
            for( int i = 0; i < Constraints.Count; ++i )
            {
                cmp = Constraints[i].CompareTo( other.Constraints[i] );
                if( cmp != 0 ) return cmp;
            }
            return cmp;
        }

        public bool Equals( TypeParameterConstraint other )
        {
            return ParameterName == other.ParameterName
                    && Constraints.Count == other.Constraints.Count
                    && Constraints.SequenceEqual( other.Constraints );
        }
        public override bool Equals( object obj )
        {
            return obj is TypeParameterConstraint o && Equals( o );
        }

        public override int GetHashCode() => _hash;
    }
}
