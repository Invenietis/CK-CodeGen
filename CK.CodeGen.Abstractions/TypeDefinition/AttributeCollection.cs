using CK.CodeGen.Abstractions;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen.SimpleParser
{
    public class AttributeCollection 
    {
        static CodeAttributeTarget[] _targets = (CodeAttributeTarget[])Enum.GetValues(typeof(CodeAttributeTarget));
        readonly AttributeSetDefinition?[] _attrs;

        public AttributeCollection()
        {
            _attrs = new AttributeSetDefinition[_targets.Length];
        }

        public AttributeSetDefinition? this[CodeAttributeTarget key]
        {
            get => _attrs[(int)key];
            set => _attrs[(int)key] = value;
        }

        /// <summary>
        /// Gets the default attribute set (the one that targets <see cref="CodeAttributeTarget.None"/>).
        /// </summary>
        public AttributeSetDefinition Default => Ensure( CodeAttributeTarget.None );

        /// <summary>
        /// Ensures that a given set of attribute exists.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public AttributeSetDefinition Ensure( CodeAttributeTarget key ) => _attrs[(int)key] ?? (_attrs[(int)key] = new AttributeSetDefinition( key ));

        /// <summary>
        /// Ensures that a given set of attribute exists either by referencing the new one
        /// or by merging its attributes into the already existing one.
        /// </summary>
        /// <param name="other">Another set of attributes.</param>
        /// <returns>The set of attributes for the <see cref="AttributeSetDefinition.Target"/>.</returns>
        public AttributeSetDefinition Ensure( AttributeSetDefinition other )
        {
            if( other == null ) throw new ArgumentNullException( nameof( other ) );
            var t = _attrs[(int)other.Target];
            if( t == null ) t = _attrs[(int)other.Target] = other;
            else
            {
                t.MergeWith( other );
            }
            return t;
        }

        /// <summary>
        /// Writes all the non null and non empty <see cref="AttributeDefinition"/>.
        /// </summary>
        /// <param name="b">The target.</param>
        /// <returns>The StringBuilder to enable fluent syntax.</returns>
        public StringBuilder Write( StringBuilder b )
        {
            foreach( var s in _attrs )
            {
                if( s != null && s.Attributes.Count > 0 )
                {
                    s.Write( b );
                }
            }
            return b;
        }

        /// <summary>
        /// Merged with another <see cref="AttributeCollection"/>.
        /// </summary>
        /// <param name="other">The other attribute collection.</param>
        /// <returns>The merged collection.</returns>
        public void MergeWith( AttributeCollection other )
        {
            for( int i = 0; i < _attrs.Length; ++i )
            {
                var t = _attrs[i];
                var o = other._attrs[i];
                if( t == null ) _attrs[i] = o;
                else if( o != null )
                {
                    t.MergeWith( o );
                }                   
            }
        }


    }
}
