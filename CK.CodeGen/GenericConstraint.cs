using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen
{
    public class GenericConstraint
    {
        public string GenericParameterName { get; set; }

        public List<string> Constraints { get; } = new List<string>();

        internal void Build(StringBuilder sb)
        {
            sb.AppendFormat(" where {0}:{1}", GenericParameterName, string.Join(",", Constraints));
        }
    }
}