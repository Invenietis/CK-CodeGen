using System;
using System.Diagnostics;
using System.Text;

namespace CK.CodeGen
{
    public class PropertyBuilder : PropertyBaseBuilder
    {
        internal PropertyBuilder(ClassBuilder classBuilder, string type, string name)
            : base(classBuilder, type, name)
        {
            PropertyType = type;
            Name = name;
            GetMethod = new PropertyMethodBuilder(this, "get");
            SetMethod = new PropertyMethodBuilder(this, "set");
        }

        public PropertyMethodBuilder GetMethod { get; }

        public PropertyMethodBuilder SetMethod { get; }

        internal override void BuildMethods(StringBuilder sb)
        {
            sb.Append("{");
            GetMethod.Build(sb);
            SetMethod.Build(sb);
            sb.Append("}");
        }
    }
}