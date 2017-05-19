using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen
{
    public class InterfaceBuilder : TypeBuilder
    {
        readonly List<PropertyDeclarationBuilder> _properties;
        readonly List<MethodDeclarationBuilder> _methods;

        internal InterfaceBuilder(NamespaceBuilder namespaceBuilder, string name)
            : base(namespaceBuilder, "interface", name)
        {
            _properties = new List<PropertyDeclarationBuilder>();
            _methods = new List<MethodDeclarationBuilder>();
        }

        public List<string> Interfaces { get; } = new List<string>();

        protected override IReadOnlyCollection<string> Parents => Interfaces;

        public IReadOnlyList<PropertyDeclarationBuilder> Properties => _properties;

        public PropertyDeclarationBuilder DefineProperty(string type, string name)
        {
            PropertyDeclarationBuilder property = new PropertyDeclarationBuilder(this, type, name);
            _properties.Add(property);
            return property;
        }

        public IReadOnlyList<MethodDeclarationBuilder> Methods => _methods;

        public MethodDeclarationBuilder DefineMethod(string name)
        {
            MethodDeclarationBuilder method = new MethodDeclarationBuilder(this, name);
            _methods.Add(method);
            return method;
        }

        protected override void BuildFields(StringBuilder b)
        {
        }

        protected override void BuildConstructors(StringBuilder b)
        {
        }

        protected override void BuildProperties(StringBuilder b)
        {
            foreach (PropertyDeclarationBuilder property in _properties) property.Build(b);
        }

        protected override void BuildMethods(StringBuilder b)
        {
            foreach (MethodDeclarationBuilder method in _methods) method.Build(b);
        }

        protected override void BuildGenericConstraints(StringBuilder b)
        {
        }
    }
}
