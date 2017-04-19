using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen
{
    public class TypeBuilder
    {
        public List<string> Attributes { get; } = new List<string>();

        public List<string> FrontModifiers { get; } = new List<string>();

        public string Name { get; set; }

        public string BaseType { get; set; }

        public IReadOnlyList<string> Interfaces { get; }

        public List<GenericConstraint> GenericConstraints { get; set; } = new List<GenericConstraint>();

        public IReadOnlyList<FieldBuilder> Fields { get; }

        public FieldBuilder DefineField(string type, string name) => new FieldBuilder(this, type, name);

        public IReadOnlyList<ConstructorBuilder> Constructors { get; }

        public ConstructorBuilder DefineConstructor() => new ConstructorBuilder(this);

        public IReadOnlyList<PropertyBaseBuilder> Properties { get; }

        public PropertyBuilder DefineProperty(string type, string name) => new PropertyBuilder(this, type, name);

        public AutoImplmentedPropertyBuilder DefineProperty(string type, string name) => new AutoImplmentedPropertyBuilder(this, type, name);

        public IReadOnlyList<MethodBuilder> Methods { get; }

        public MethodBuilder DefineMethod(string frontModifiers, string name) => new MethodBuilder(this, frontModifiers, name);

        public MethodBuilder DefineMethod(string name) => DefineMethod(null, name);

        public StringBuilder ExtraBody { get; } = new StringBuilder();

    }
}