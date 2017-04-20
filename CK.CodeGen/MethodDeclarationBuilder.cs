using System;
using System.Text;

namespace CK.CodeGen
{
    public class MethodDeclarationBuilder : MethodBaseBuilder
    {
        readonly InterfaceBuilder _interface;

        internal MethodDeclarationBuilder(InterfaceBuilder interfaceBuilder, string name)
            : base(interfaceBuilder, name)
        {
            _interface = interfaceBuilder;
        }

        protected override void BuildFrontModifiers(StringBuilder sb)
        {
        }

        internal override void BuildBody(StringBuilder sb)
        {
            sb.Append(";");
        }
    }
}