using CK.CodeGen.Abstractions;
using CK.CodeGen.SimpleParser;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen
{
    class MethodDefinition
    {
        [Flags]
        public enum ParameterModifier
        {
            None,
            Out = 1<<0,
            Ref = 1<<1,
            Params = 1<<2,
            This = 1 << 3,
            In = 1 << 4
        }

        public class Parameter
        {
            public Parameter(
                AttributeCollection? attributes,
                ParameterModifier modifiers,
                TypeName type,
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

            public AttributeCollection Attributes { get; }

            public ParameterModifier Modifiers { get; }

            public TypeName Type { get; }

            public string Name { get; }

            public string? DefaultValue { get; }
        }

        public MethodDefinition(
            AttributeCollection? attributes,
            Modifiers modifiers,
            ExtendedTypeName? returnType,
            TypeName methodName,
            bool isIndexer,
            IReadOnlyList<Parameter>? parameters,
            IReadOnlyList<TypeParameterConstraint>? constraints
            )
        {
            Attributes = attributes ?? new AttributeCollection();
            Modifiers = Modifiers;
            ReturnType = returnType;
            MethodName = methodName;
            Parameters = parameters ?? Array.Empty<Parameter>();
            Constraints = constraints ?? Array.Empty<TypeParameterConstraint>();
        }

        public AttributeCollection Attributes { get; }

        public Modifiers Modifiers { get; }

        public ExtendedTypeName? ReturnType { get; }

        public TypeName MethodName { get; }

        public StringBuilder WriteParameters( StringBuilder b, bool withAttributes, bool withDefaultValues )
        {
            b.Append( '(' );
            bool already = false;
            foreach( var p in Parameters )
            {
                if( already ) b.Append( ", " );
                else already = true;
                if( withAttributes ) p.Attributes.Write( b );
                switch( p.Modifiers )
                {
                    case ParameterModifier.Out: b.Append( "out " ); break;
                    case ParameterModifier.This: b.Append( "this " ); break;
                    case ParameterModifier.Ref: b.Append( "ref " ); break;
                    case ParameterModifier.Params: b.Append( "params " ); break;
                    case ParameterModifier.In: b.Append( "in " ); break;
                }
                p.Type.Write( b ).Append( ' ' ).Append( p.Name );
                if( withDefaultValues && !String.IsNullOrEmpty( p.DefaultValue ) )
                {
                    b.Append( " = " ).Append( p.DefaultValue );
                }
            }
            b.Append( ')' );
            return b;
        }

        public bool IsIndexer { get; }

        public IReadOnlyList<Parameter> Parameters { get; }

        public IReadOnlyList<TypeParameterConstraint> Constraints { get; }

    }
}
