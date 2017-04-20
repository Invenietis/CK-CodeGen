using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.CodeGen
{
    public class ClassBuilder : TypeBuilder
    {
        readonly List<FieldBuilder> _fields;
        readonly List<ConstructorBuilder> _constructors;
        readonly List<PropertyBaseBuilder> _properties;
        readonly List<MethodBuilder> _methods;

        internal ClassBuilder(NamespaceBuilder namespaceBuilder, string name)
            : this(namespaceBuilder, "class", name)
        {
        }

        internal ClassBuilder(NamespaceBuilder namespaceBuilder, string type, string name)
            : base(namespaceBuilder, type, name)
        {
            _fields = new List<FieldBuilder>();
            _constructors = new List<ConstructorBuilder>();
            _properties = new List<PropertyBaseBuilder>();
            _methods = new List<MethodBuilder>();
        }

        public string BaseType { get; set; }

        public List<string> Interfaces { get; } = new List<string>();

        protected override IReadOnlyCollection<string> Parents
        {
            get
            {
                List<string> parents = new List<string>();
                if (HasBaseType) parents.Add(BaseType);
                if (HasInterface) parents.AddRange(Interfaces);
                return parents;
            }
        }

        public List<GenericConstraint> GenericConstraints { get; } = new List<GenericConstraint>();

        public IReadOnlyList<FieldBuilder> Fields => _fields;

        public FieldBuilder DefineField(string type, string name)
        {
            FieldBuilder field = new FieldBuilder(this, type, name);
            _fields.Add(field);
            return field;
        }

        public IReadOnlyList<ConstructorBuilder> Constructors => _constructors;

        public ConstructorBuilder DefineConstructor()
        {
            ConstructorBuilder constructor = new ConstructorBuilder(this);
            _constructors.Add(constructor);
            return constructor;
        }

        public IReadOnlyList<PropertyBaseBuilder> Properties => _properties;

        public PropertyBuilder DefineProperty(string type, string name)
        {
            PropertyBuilder property = new PropertyBuilder(this, type, name);
            _properties.Add(property);
            return property;
        }

        public AutoImplmentedPropertyBuilder DefineAutoImplementedProperty(string type, string name)
        {
            AutoImplmentedPropertyBuilder property = new AutoImplmentedPropertyBuilder(this, type, name);
            _properties.Add(property);
            return property;
        }

        public IReadOnlyList<MethodBuilder> Methods => _methods;

        public MethodBuilder DefineMethod(string frontModifiers, string name)
        {
            MethodBuilder method = new MethodBuilder(this, frontModifiers, name);
            _methods.Add(method);
            return method;
        }

        public MethodBuilder DefineMethod(string name) => DefineMethod(null, name);

        protected override void BuildFields(StringBuilder sb)
        {
            foreach (FieldBuilder field in Fields) field.Build(sb);
        }

        protected override void BuildConstructors(StringBuilder sb)
        {
            foreach (ConstructorBuilder constructor in Constructors) constructor.Build(sb);
        }

        protected override void BuildProperties(StringBuilder sb)
        {
            foreach (PropertyBaseBuilder property in Properties) property.Build(sb);
        }

        protected override void BuildMethods(StringBuilder sb)
        {
            foreach (MethodBuilder method in Methods) method.Build(sb);
        }

        protected override void BuildGenericConstraints(StringBuilder sb)
        {
            foreach (GenericConstraint constraint in GenericConstraints) constraint.Build(sb);
        }

        bool HasBaseType => !string.IsNullOrEmpty(BaseType);

        bool HasInterface => Interfaces.Count > 0;
    }
}