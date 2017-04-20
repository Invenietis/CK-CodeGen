using System;
using System.Text;

namespace CK.CodeGen
{
    public class PropertyDeclarationBuilder : PropertyBaseBuilder
    {
        internal PropertyDeclarationBuilder(InterfaceBuilder interfaceBuilder, string type, string name)
            : base(interfaceBuilder, type, name)
        {
        }

        public bool HasGetter { get; set; } = true;

        public bool HasSetter { get; set; } = true;

        internal override void BuildMethods(StringBuilder sb)
        {
            if (!(HasGetter || HasSetter)) throw new InvalidOperationException("A property must have at least one getter or one setter.");
            sb.Append("{");
            if (HasGetter) sb.Append("get;");
            if (HasSetter) sb.Append("set;");
            sb.Append("}");
        }
    }
}