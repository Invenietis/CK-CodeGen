using System;
using System.Text;

namespace CK.CodeGen
{
    public class EnumValue
    {
        public string Name { get; set; }

        public string Value { get; set; }

        internal void Build(StringBuilder sb)
        {
            sb.Append(Name);
            if (HasValue) sb.AppendFormat("={0}", Value);
        }

        bool HasValue => !string.IsNullOrEmpty(Value);
    }
}
