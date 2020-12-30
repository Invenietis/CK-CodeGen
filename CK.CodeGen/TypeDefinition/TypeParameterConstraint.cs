using CK.CodeGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.CodeGen
{
    /// <summary>
    /// Mutable generic type constraint "where <see cref="ParameterName"/> : <see cref="Constraints"/>".
    /// </summary>
    public class TypeParameterConstraint
    {
        /// <summary>
        /// Gets the name of the generic type parameter that is constrained.
        /// </summary>
        public string ParameterName { get; }

        /// <summary>
        /// Gets a mutable list of type names (that can be simple identifiers like "notnull").
        /// </summary>
        public List<ExtendedTypeName> Constraints { get; }

        /// <summary>
        /// Initializes a new <see cref="TypeParameterConstraint"/> with an optional initial list
        /// of <see cref="Constraints"/>.
        /// </summary>
        /// <param name="name">The <see cref="ParameterName"/>.</param>
        /// <param name="constraints">Optional list of constraints.</param>
        public TypeParameterConstraint( string name, List<ExtendedTypeName>? constraints = null )
        {
            if( String.IsNullOrWhiteSpace( name ) ) throw new ArgumentOutOfRangeException( nameof( name ) );
            ParameterName = name;
            Constraints = constraints ?? new List<ExtendedTypeName>();
        }

        /// <summary>
        /// Writes this TypeParameterConstraint into the provided StringBuilder.
        /// </summary>
        /// <param name="b">The target.</param>
        /// <returns>The StringBuilder to enable fluent syntax.</returns>
        public StringBuilder Write( StringBuilder b )
        {
            if( Constraints.Count > 0 )
            {
                b.Append( "where " ).Append( ParameterName ).Append( " : " );
                bool already = false;
                foreach( var t in Constraints )
                {
                    if( already ) b.Append( ", " );
                    else already = true;
                    t.Write( b );
                }
            }
            return b;
        }
    }
}
