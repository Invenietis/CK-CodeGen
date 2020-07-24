using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen
{
    /// <summary>
    /// Captures final complete information about Type Reference nullability.
    /// This encapsulates a Type path to a final type: as long as a Type is not a nested type, there is
    /// only one <see cref="NullableTypeTree"/> in it.
    /// </summary>
    public readonly struct NullableTypeInfo : IReadOnlyList<NullableTypeTree>
    {
        readonly NullableTypeTree[] _path;

        internal NullableTypeInfo( NullableTypeTree[] path )
        {
            _path = path;
        }

        /// <summary>
        /// Gets the <see cref="NullableTypeTree"/> in the path at the given index,
        /// starting at the top enclosing type up to the final, nested, type.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The type tree.</returns>
        public NullableTypeTree this[int index] => _path[index];

        /// <summary>
        /// Gets the number of trees in the type path. For non nested type this is always 1.
        /// </summary>
        public int Count => _path.Length;

        /// <summary>
        /// Gets an enumerator of trees starting at the top enclosing type up to the final, nested, type.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<NullableTypeTree> GetEnumerator() => (IEnumerator<NullableTypeTree>)_path.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _path.GetEnumerator();

        /// <summary>
        /// </summary>
        /// <param name="b">The string builder to use.</param>
        /// <returns>The string builder.</returns>
        public StringBuilder ToString( StringBuilder b )
        {
            bool atLeastOne = false;
            foreach( var t in _path )
            {
                if( atLeastOne ) b.Append( '.' );
                else atLeastOne = true;
                t.ToString( b );
            }
            return b;
        }

        /// <summary>
        /// Calls <see cref="ToString(StringBuilder)"/> and returns the result.
        /// </summary>
        /// <returns>A readable string.</returns>
        public override string ToString() => ToString( new StringBuilder() ).ToString();

    }
}
