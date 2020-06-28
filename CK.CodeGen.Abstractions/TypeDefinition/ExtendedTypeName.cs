using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace CK.CodeGen.Abstractions
{
    /// <summary>
    /// Unifies <see cref="TypeName"/> and <see cref="TupleTypeName"/> (simple algebraic data type).
    /// </summary>
    public class ExtendedTypeName
    {
        /// <summary>
        /// Gets an empty type name instance.
        /// </summary>
        public static readonly ExtendedTypeName Empty = new ExtendedTypeName( TypeName.Empty );

        /// <summary>
        /// Initializes a new <see cref="ExtendedTypeName"/> for a  <see cref="TupleTypeName"/>.
        /// </summary>
        /// <param name="tuple">The tuple.</param>
        public ExtendedTypeName( TupleTypeName tuple )
        {
            TupleTypeName = tuple ?? throw new ArgumentNullException( nameof( tuple ) );
        }

        /// <summary>
        /// Initializes a new <see cref="ExtendedTypeName"/> for a  <see cref="TypeName"/>.
        /// </summary>
        /// <param name="type">The type.</param>
        public ExtendedTypeName( TypeName type )
        {
            TypeName = type ?? throw new ArgumentNullException( nameof( type ) );
        }

        /// <summary>
        /// Gets wether this is a tuple or a type name.
        /// </summary>
        [MemberNotNullWhen(true, nameof( TupleTypeName ) )]
        [MemberNotNullWhen(false, nameof( TypeName ) )]
        public bool IsTuple => TypeName! == null;

        /// <summary>
        /// Gets the type name or null if this is a <see cref="TupleTypeName"/>.
        /// </summary>
        public TypeName? TypeName { get; }

        /// <summary>
        /// Gets the tuple type name or null if this is a <see cref="TypeName"/>.
        /// </summary>
        public TupleTypeName? TupleTypeName { get; }

        /// <summary>
        /// Writes this ExtendedTypeName into the provided StringBuilder.
        /// </summary>
        /// <param name="b">The target.</param>
        /// <returns>The StringBuilder to enable fluent syntax.</returns>
        public StringBuilder Write( StringBuilder b )
        {
            if( IsTuple ) TupleTypeName.Write( b );
            else TypeName.Write( b );
            return b;
        }

        /// <summary>
        /// Overridden to return the <see cref="Write(StringBuilder)"/> result.
        /// </summary>
        /// <returns>The type string.</returns>
        public override string ToString() => Write( new StringBuilder() ).ToString();

    }
}
