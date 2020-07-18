using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CK.CodeGen.Abstractions
{
    /// <summary>
    /// Attribute definition is a <see cref="TypeName"/> and an optional
    /// comma separated list of constructor arguments.
    /// <para>
    /// Currently, this list of arguments also contains property initializations (Property = value).
    /// A "Properties" list of (PropertyNames,Value) should be defined if needed.
    /// </para>
    /// </summary>
    public class AttributeDefinition
    {
        /// <summary>
        /// Initializes a new <see cref="AttributeDefinition"/>.
        /// </summary>
        /// <param name="name">The attribute type name.</param>
        /// <param name="constructorArguments">Optional initial <see cref="ConstructorArguments"/> list.</param>
        public AttributeDefinition( TypeName name, List<string>? constructorArguments = null )
        {
            TypeName = name ?? throw new ArgumentNullException( nameof( name ) );
            ConstructorArguments = constructorArguments ?? new List<string>();
        }

        /// <summary>
        /// Gets the type of this attributes.
        /// </summary>
        public TypeName TypeName { get; }

        /// <summary>
        /// Gets a mutable list of constructor arguments (and/or properties initialization currently).
        /// </summary>
        public IList<string> ConstructorArguments { get; }

        /// <summary>
        /// Writes this AttributeDefinition into the provided StringBuilder.
        /// </summary>
        /// <param name="b">The target.</param>
        /// <returns>The StringBuilder to enable fluent syntax.</returns>
        public StringBuilder Write( StringBuilder b )
        {
            TypeName.Write( b );
            if( ConstructorArguments.Count > 0 )
            {
                b.Append( '(' ).AppendJoin( ",", ConstructorArguments ).Append( ')' );
            }
            return b;
        }
    }

}
