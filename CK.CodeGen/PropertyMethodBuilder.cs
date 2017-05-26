using System.Text;

namespace CK.CodeGen
{
    public class PropertyMethodBuilder
    {
        readonly PropertyBuilder _property;
        readonly string _type;

        internal PropertyMethodBuilder(PropertyBuilder propertyBuilder, string type)
        {
            _property = propertyBuilder;
            _type = type;
        }

        public string FrontModifier { get; set; }

        public StringBuilder Body { get; } = new StringBuilder();

        internal void Build(StringBuilder sb)
        {
            if (HasBody)
            {
                if (HasFrontModifier) sb.AppendWithWhitespace(FrontModifier);
                sb.AppendWithWhitespace(_type);
                BuildHelpers.BuildMethodBody(sb, Body.ToString());
            }
        }

        bool HasBody => Body.Length > 0;

        bool HasFrontModifier => FrontModifier?.Length > 0;
    }
}