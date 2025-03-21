using System;
using System.Linq;
using System.Text;
using CK.Core;

namespace CK.CodeGen.SimpleParser;

/// <summary>
/// Handles <see cref="AttributeSetDefinition"/> grouped by their <see cref="AttributeSetDefinition.Target"/>.
/// </summary>
public sealed class AttributeCollection
{
    static CodeAttributeTarget[] _targets = (CodeAttributeTarget[])Enum.GetValues( typeof( CodeAttributeTarget ) );
    readonly AttributeSetDefinition?[] _attrs;

    /// <summary>
    /// Initializes a new empty <see cref="AttributeCollection"/>.
    /// </summary>
    public AttributeCollection()
    {
        _attrs = new AttributeSetDefinition[_targets.Length];
    }

    /// <summary>
    /// Gets whether at least one attribute is defined.
    /// </summary>
    public bool HasAttributes => _attrs.Any( a => a != null && a.Attributes.Count > 0 );

    /// <summary>
    /// Gets the attributes for a <see cref="CodeAttributeTarget"/> or null.
    /// </summary>
    /// <param name="key">The target.</param>
    /// <returns>The attributes or null.</returns>
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
        Throw.CheckNotNullArgument( other );
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
        Throw.CheckNotNullArgument( b );
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
        Throw.CheckNotNullArgument( other );
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
