using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen
{
    public class NamespaceBuilder
    {
        readonly List<TypeBuilder> _types = new List<TypeBuilder>();

        public NamespaceBuilder(string ns)
        {
            Name = ns;
        }

        public List<string> Usings { get; } = new List<string>();

        public IReadOnlyList<TypeBuilder> Types => _types;

        public string Name { get; set; }

        public InterfaceBuilder DefineInterface(string name) => DefineType(name, n => new InterfaceBuilder(this, n));

        public ClassBuilder DefineClass(string name) => DefineType(name, n => new ClassBuilder(this, n));

        public EnumBuilder DefineEnum(string name) => DefineType(name, n => new EnumBuilder(this, n));

        public StructBuilder DefineStruct(string name) => DefineType(name, n => new StructBuilder(this, n));

        T DefineType<T>(string name, Func<string, T> factory) where T : TypeBuilder
        {
            T builder = factory(name);
            _types.Add(builder);
            return builder;
        }

        public string CreateSource()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("namespace {0}", Name).Append("{");
            BuildUsings(sb);
            BuildTypes(sb);
            sb.Append("}");
            return sb.ToString();
        }

        void BuildUsings(StringBuilder sb)
        {
            foreach (string u in Usings)
                if (u.StartsWith("using")) sb.Append(u);
                else sb.AppendFormat("using {0};", u);
        }

        void BuildTypes(StringBuilder sb)
        {
            foreach (TypeBuilder typeBuilder in _types) typeBuilder.Build(sb);
        }
    }
}
