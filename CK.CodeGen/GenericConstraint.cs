using System;
using System.Collections.Generic;
using System.Text;
using CK.Text;

namespace CK.CodeGen
{
    public class GenericConstraint
    {
        public string GenericParameterName { get; set; }

        public List<string> Constraints { get; } = new List<string>();

        internal void Build(StringBuilder b)
        {
            b.Append("where ")
            .Append(GenericParameterName)
            .Append(":")
            .AppendStrings(Constraints)
            .AppendLine();
        }
    }
}