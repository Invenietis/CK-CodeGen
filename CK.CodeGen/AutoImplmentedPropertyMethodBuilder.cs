using System;
using System.Text;

namespace CK.CodeGen
{
    public class AutoImplmentedPropertyMethodBuilder
    {
        readonly AutoImplmentedPropertyBuilder _propertyBuilder;
        readonly string _type;

        internal AutoImplmentedPropertyMethodBuilder(AutoImplmentedPropertyBuilder propertyBuilder, string type)
        {
            _propertyBuilder = propertyBuilder;
            _type = type;
        }

        public string FrontModifier { get; set; }

        public bool Exists { get; set; } = true;

        internal void Build(StringBuilder sb)
        {
            if (Exists) sb.AppendWithWhitespace(FrontModifier).AppendFormat("{0};", _type);
        }
    }
}