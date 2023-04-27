using CK.CodeGen.SimpleParser;

namespace CK.CodeGen
{
    public partial class FunctionDefinition
    {
        /// <summary>
        /// Nearly immutable parameter definition: only the <see cref="Attributes"/> and the
        /// <see cref="DefaultValue"/> are mutables.
        /// </summary>
        public class Parameter
        {
            /// <summary>
            /// Initializes a new <see cref="Parameter"/>.
            /// </summary>
            /// <param name="attributes">The <see cref="Attributes"/>.</param>
            /// <param name="modifiers">The <see cref="Modifiers"/>.</param>
            /// <param name="type">The parameter's type.</param>
            /// <param name="name">The parameter's name.</param>
            /// <param name="defaultValue">The <see cref="DefaultValue"/>.</param>
            public Parameter(
                AttributeCollection? attributes,
                ParameterModifier modifiers,
                ExtendedTypeName type,
                string name,
                string? defaultValue
                )
            {
                Attributes = attributes ?? new AttributeCollection();
                Modifiers = modifiers;
                Type = type;
                Name = name;
                DefaultValue = defaultValue;
            }

            /// <summary>
            /// Gets the mutable set of parameter's attributes.
            /// </summary>
            public AttributeCollection Attributes { get; }

            /// <summary>
            /// Gets the parameter's modifiers (.
            /// </summary>
            public ParameterModifier Modifiers { get; }

            /// <summary>
            /// Gets the type name.
            /// </summary>
            public ExtendedTypeName Type { get; }

            /// <summary>
            /// Gets the parameter name.
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// Gets or sets the parameter's default value.
            /// </summary>
            public string? DefaultValue { get; set; }
        }
    }
}
