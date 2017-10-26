using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.CodeGen
{
    class TypeName : IComparable<TypeName>, IEquatable<TypeName>
    {
        public enum VariantModifier
        {
            None,
            In,
            Out
        }

        public struct GenParam : IComparable<GenParam>, IEquatable<GenParam>
        {
            public readonly VariantModifier Variance;
            public readonly TypeName Type;

            public static readonly GenParam Empty = new TypeName.GenParam( VariantModifier.None, TypeName.Empty );

            internal GenParam( VariantModifier v, TypeName t )
            {
                Variance = v;
                Type = t;
            }

            public int CompareTo( GenParam other )
            {
                int cmp = Variance - other.Variance;
                if( cmp != 0 ) return cmp;
                return Type.CompareTo( other.Type );
            }

            public bool Equals( GenParam other )
            {
                return Variance == other.Variance && Type.Equals( other.Type );
            }

            public override bool Equals( object obj )
            {
                return obj is GenParam o && Equals( o );
            }

            public override int GetHashCode()
            {
                return Type.GetHashCode() ^ ((int)Variance) << 16;
            }

            internal void Write( StringBuilder b )
            {
                if( Variance == VariantModifier.In ) b.Append( "in " );
                else if( Variance == VariantModifier.Out ) b.Append( "out " );
                Type.Write( b );
            }
        }

        public static readonly TypeName Empty = new TypeName( String.Empty, null, null );

        readonly string _name;
        readonly IReadOnlyList<GenParam> _genArgs;
        readonly IReadOnlyList<int> _arrayDims;
        readonly int _hash;

        internal TypeName( string n, IReadOnlyList<GenParam> gen, IReadOnlyList<int> arrayDims )
        {
            _name = n;
            _genArgs = gen ?? Array.Empty<GenParam>();
            _arrayDims = arrayDims ?? Array.Empty<int>();
            _hash = _name.GetHashCode();
            foreach( var d in _arrayDims ) _hash = _hash << 1 ^ (int)d;
            foreach( var g in _genArgs ) _hash = _hash ^ g.GetHashCode();
            TypeKey = _name + '`' + _genArgs.Count;
        }

        public string Name => _name;

        public IReadOnlyList<GenParam> GenArgs => _genArgs;

        public IReadOnlyList<int> ArrayDims => _arrayDims;

        public string TypeKey { get; }

        public StringBuilder Write( StringBuilder b )
        {
            b.Append( _name );
            if( _genArgs.Count > 0 )
            {
                b.Append( '<' );
                bool already = false;
                foreach( var g in _genArgs )
                {
                    if( already ) b.Append( ',' );
                    else already = true;
                    g.Write( b );
                }
                b.Append( '>' );
            }
            foreach( int d in _arrayDims )
            {
                b.Append( '[' ).Append( ',', d ).Append( ']' );
            }
            return b;
        }

        public int CompareTo( TypeName other )
        {
            int cmp = StringComparer.Ordinal.Compare( _name, other._name );
            if( cmp != 0 ) return _name == "new()" ? 1 : (other._name == "new()" ? -1 : cmp);
            cmp = _genArgs.Count - other._genArgs.Count;
            if( cmp != 0 ) return cmp;
            cmp = _arrayDims.Count - other._arrayDims.Count;
            if( cmp != 0 ) return cmp;
            for( int i = 0; i < _arrayDims.Count; ++i )
            {
                cmp = _arrayDims[i] - other._arrayDims[i];
                if( cmp != 0 ) return cmp;
            }
            for( int i = 0; i < _genArgs.Count; ++i )
            {
                cmp = _genArgs[i].CompareTo( other._genArgs[i] );
                if( cmp != 0 ) return cmp;
            }
            return cmp;
        }

        public bool Equals( TypeName other )
        {
            return _name == other._name
                    && _genArgs.Count == other._genArgs.Count
                    && _arrayDims.Count == other._arrayDims.Count
                    && _arrayDims.SequenceEqual( other._arrayDims )
                    && _genArgs.SequenceEqual( other._genArgs );
        }

        public override bool Equals( object obj ) => obj is TypeName o && Equals( o );

        public override int GetHashCode() => _hash;

        public override string ToString() => Write( new StringBuilder() ).ToString();

    }

}
