using CK.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen;

/// <summary>
/// Defines a list of <see cref="Field"/>.
/// </summary>
public class TupleTypeName
{
    /// <summary>
    /// Defines a tuple field.
    /// </summary>
    /// <param name="FieldType"> Gets the type of the field (that may be itself a <see cref="TupleTypeName"/>). </param>
    /// <param name="FieldName"> Gets the optional field name. </param>
    public readonly record struct Field( ExtendedTypeName FieldType, string? FieldName = null )
    {
        /// <summary>
        /// Writes this Field into the provided StringBuilder.
        /// </summary>
        /// <param name="b">The target.</param>
        /// <param name="typeNameReplacer">Optional naked type name replacer function.</param>
        /// <returns>The StringBuilder to enable fluent syntax.</returns>
        public StringBuilder Write( StringBuilder b, Func<string, string>? typeNameReplacer = null )
        {
            FieldType.Write( b, typeNameReplacer );
            if( !String.IsNullOrEmpty( FieldName ) )
            {
                b.Append( ' ' );
                b.Append( FieldName );
            }
            return b;
        }
    }

    /// <summary>
    /// Gets the mutable list of fields.
    /// </summary>
    public IList<Field> Fields { get; }

    /// <summary>
    /// Initializes a new <see cref="TupleTypeName"/> with an optional initial <see cref="Fields"/> list.
    /// </summary>
    /// <param name="fields">Optional initial <see cref="Fields"/> list.</param>
    public TupleTypeName( IList<Field>? fields = null )
    {
        Fields = fields ?? new List<Field>();
    }

    /// <summary>
    /// Writes this ExtendedTypeName into the provided StringBuilder.
    /// </summary>
    /// <param name="b">The target.</param>
    /// <param name="typeNameReplacer">Optional naked type name replacer function.</param>
    /// <returns>The StringBuilder to enable fluent syntax.</returns>
    public StringBuilder Write( StringBuilder b, Func<string, string>? typeNameReplacer = null )
    {
        Throw.CheckNotNullArgument( b );
        b.Append( '(' );
        bool atLeastOne = false;
        foreach( var f in Fields )
        {
            if( atLeastOne ) b.Append( ',' );
            else atLeastOne = true;
            f.Write( b, typeNameReplacer );
        }
        return b.Append( ')' );
    }

    /// <summary>
    /// Overridden to return the <see cref="Write"/> result.
    /// </summary>
    /// <returns>The type string.</returns>
    public override string ToString() => Write( new StringBuilder() ).ToString();

}
