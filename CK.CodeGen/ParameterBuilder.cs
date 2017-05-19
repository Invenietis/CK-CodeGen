using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen
{
    public class ParameterBuilder
    {
        public List<string> Attributes { get; } = new List<string>();

        public string ParameterType { get; set; }

        public string Name { get; set; }

        public string DefaultValue { get; set; }

        internal void Build(StringBuilder sb)
        {
            BuildHelpers.BuildAttributes(sb, Attributes);
            sb.AppendFormat("{0} {1}", ParameterType, Name);
            if (HasDefaultValue) sb.AppendFormat("={0}", DefaultValue);
        }

        bool HasDefaultValue => !string.IsNullOrEmpty(DefaultValue);
    }
}