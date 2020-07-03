using CK.CodeGen;
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
            CallConstructor thisOrBaseConstructorCall,
            string? thisOrBaseConstructorParameters,
            bool isIndexer,
            IReadOnlyList<Parameter>? parameters,
            IReadOnlyList<TypeParameterConstraint>? constraints
            )
        {
            Attributes = attributes ?? new AttributeCollection();
            Modifiers = modifiers;
            ReturnType = returnType;
            MethodName = methodName;
            ThisOrBaseConstructorCall = thisOrBaseConstructorCall;
            ThisOrBaseConstructorParameters = thisOrBaseConstructorParameters;
            Parameters = parameters ?? Array.Empty<Parameter>();
            Constraints = constraints ?? Array.Empty<TypeParameterConstraint>();
        }

        public ExtendedTypeName? ReturnType { get; }

        public AttributeCollection Attributes { get; }

        public Modifiers Modifiers { get; }

        public TypeName MethodName { get; }

        public enum CallConstructor
        {
            None,
            This,
            Base
        }

        public CallConstructor ThisOrBaseConstructorCall { get; set; }

        public string? ThisOrBaseConstructorParameters { get; set; }

        public bool IsIndexer { get; }

        public IReadOnlyList<Parameter> Parameters { get; }

        public IReadOnlyList<TypeParameterConstraint> Constraints { get; }

        public StringBuilder WriteParameters( StringBuilder b, bool withAttributes, bool withDefaultValues )
        {
            b.Append( '(' );
            if( Parameters.Count > 0 )
            {
                b.Append( ' ' );
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
                b.Append( ' ' );
            }
            b.Append( ')' );
            return b;
        }

        /// <summary>
        /// Writes this TypeDefinition into the provided StringBuilder.
        /// </summary>
        /// <param name="b">The target.</param>
        /// <returns>The StringBuilder to enable fluent syntax.</returns>
        public StringBuilder Write( StringBuilder b )
        {
            if( Attributes.HasAttributes )
            {
                Attributes.Write( b );
                b.AppendLine();
            }
            Modifiers.Write( b );
            if( ReturnType != null )
            {
                ReturnType.Write( b );
                b.Append( ' ' );
            }
            MethodName.Write( b );
            WriteParameters( b, true, true );
            if( Constraints.Count > 0 )
            {
                b.Append( ' ' );
                bool already = false;
                foreach( var c in Constraints )
                {
                    if( already ) b.Append( ' ' );
                    else already = true;
                    c.Write( b );
                }
            }
            if( ThisOrBaseConstructorCall != CallConstructor.None )
            {
                b.Append( " : " )
                 .Append( ThisOrBaseConstructorCall == CallConstructor.This ? "this" : "base" )
                 .Append( '(' )
                 .Append( ThisOrBaseConstructorParameters )
                 .Append( ')' );
            }
            return b;
        }

    }
}
