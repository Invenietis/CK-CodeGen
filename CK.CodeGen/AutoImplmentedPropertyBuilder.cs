using System;
using System.Text;

namespace CK.CodeGen
{
    public class AutoImplmentedPropertyBuilder : PropertyBaseBuilder
    {
        internal AutoImplmentedPropertyBuilder(ClassBuilder classBuilder, string type, string name)
            : base(classBuilder, type, name)
        {
            Type = type;
            Name = name;
            Getter = new AutoImplmentedPropertyMethodBuilder(this, "get");
            Setter = new AutoImplmentedPropertyMethodBuilder(this, "set");
        }

        public AutoImplmentedPropertyMethodBuilder Getter { get; }

        public AutoImplmentedPropertyMethodBuilder Setter { get; }

        public string InitialValue { get; set; }

        internal override void BuildMethods(StringBuilder sb)
        {
            sb.Append("{");
            Getter.Build(sb);
            Setter.Build(sb);
            sb.Append("}");
            if (HasInitialValue) sb.AppendFormat("={0};", InitialValue);
        }

        bool HasInitialValue => !string.IsNullOrEmpty(InitialValue);
    }
}