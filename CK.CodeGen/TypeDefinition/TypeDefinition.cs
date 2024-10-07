using CK.CodeGen;
using CK.CodeGen.SimpleParser;
using CK.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.CodeGen;

/// <summary>
/// Type definition includes <see cref="Attributes"/>, <see cref="Modifiers"/>, <see cref="Kind"/>, <see cref="TypeName"/> that identifies this definition
/// and cannot be changed, <see cref="BaseTypes"/> and <see cref="Constraints"/>.
/// </summary>
public class TypeDefinition
{
    /// <summary>
    /// Simple enumeration that identifies class, interface, enum or structs.
    /// </summary>
    public enum TypeKind
    {
        /// <summary>
        /// Non applicable.
        /// </summary>
        None,

        /// <summary>
        /// The type is a class.
        /// </summary>
        Class,

        /// <summary>
        /// The type is an interface.
        /// </summary>
        Interface,

        /// <summary>
        /// The type is an enum.
        /// </summary>
        Enum,

        /// <summary>
        /// The type is a struct.
        /// </summary>
        Struct
    }

    /// <summary>
    /// Gets a mutable set of attributes grouped by <see cref="CodeAttributeTarget"/>.
    /// </summary>
    public AttributeCollection Attributes { get; }

    /// <summary>
    /// Gets or sets the modifiers for this definition.
    /// </summary>
    public Modifiers Modifiers { get; set; }

    /// <summary>
    /// Gets or sets whether this is a class, struct, enum or interface definition.
    /// </summary>
    public TypeKind Kind { get; set; }

    /// <summary>
    /// Gets the immutable type name that identifies this definition.
    /// </summary>
    public TypeName Name { get; }

    /// <summary>
    /// Gets a mutable list of base types.
    /// </summary>
    public IList<ExtendedTypeName> BaseTypes { get; }

    /// <summary>
    /// Gets a mutable list of generic constraints.
    /// </summary>
    public IList<TypeParameterConstraint> Constraints { get; }

    /// <summary>
    /// Initializes a new <see cref="TypeDefinition"/>.
    /// </summary>
    /// <param name="attributes">Optional attribute collection.</param>
    /// <param name="modifiers">Type modifiers.</param>
    /// <param name="kind">Type kind.</param>
    /// <param name="name">Naked type name.</param>
    /// <param name="bases">Optional list of base types.</param>
    /// <param name="constraints">Optional list of constraints.</param>
    public TypeDefinition( AttributeCollection? attributes,
                           Modifiers modifiers,
                           TypeKind kind,
                           TypeName name,
                           IList<ExtendedTypeName>? bases,
                           IList<TypeParameterConstraint>? constraints )
    {
        Throw.CheckNotNullArgument( name );
        Attributes = attributes ?? new AttributeCollection();
        Modifiers = modifiers;
        Kind = kind;
        Name = name;
        BaseTypes = bases ?? new List<ExtendedTypeName>();
        Constraints = constraints ?? new List<TypeParameterConstraint>();
    }

    /// <summary>
    /// Writes this TypeDefinition into the provided StringBuilder.
    /// </summary>
    /// <param name="b">The target.</param>
    /// <returns>The StringBuilder to enable fluent syntax.</returns>
    public StringBuilder Write( StringBuilder b )
    {
        Throw.CheckNotNullArgument( b );
        Attributes.Write( b );
        Modifiers.Write( b );
        switch( Kind )
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

    /// <summary>
    /// To be merged with another type definition, <see cref="Kind"/> and <see cref="TypeName.Key"/> must be the same
    /// otherwise an <see cref="InvalidOperationException"/> is thrown.
    /// </summary>
    /// <param name="other">The other type definition.</param>
    /// <returns>The merged definition.</returns>
    public void MergeWith( TypeDefinition other )
    {
        Throw.CheckNotNullArgument( other );
        if( Kind != other.Kind )
        {
            Throw.InvalidOperationException( $"Unable to merge type '{ToString()}' with '{other}': Kind differ {Kind} vs. {other.Kind}." );
        }
        if( Name.Key != other.Name.Key )
        {
            Throw.InvalidOperationException( $"Unable to merge type '{ToString()}' with '{other}': TypeDefinitionKey differ {Name.Key} vs. {other.Name.Key}." );
        }

        Attributes.MergeWith( other.Attributes );
        Modifiers |= other.Modifiers;

        var baseTypes = new HashSet<ExtendedTypeName>( BaseTypes.Concat( other.BaseTypes ) );
        BaseTypes.Clear();
        BaseTypes.AddRange( baseTypes );

        var constraints = new HashSet<TypeParameterConstraint>( Constraints.Concat( other.Constraints ) );
        Constraints.Clear();
        Constraints.AddRange( constraints );
    }


    /// <summary>
    /// Overridden to return the <see cref="Write(StringBuilder)"/> result.
    /// </summary>
    /// <returns>The type string.</returns>
    public override string ToString() => Write( new StringBuilder() ).ToString();

}
