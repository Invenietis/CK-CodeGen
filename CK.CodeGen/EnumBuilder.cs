using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen
{
    public class EnumBuilder : TypeBuilder
    {
        internal EnumBuilder(NamespaceBuilder namespaceBuilder, string name)
            : base(namespaceBuilder, "enum", name)
        {
        }

        protected override IReadOnlyCollection<string> Parents { get; } = new string[0];

        public List<EnumValue> Values { get; } = new List<EnumValue>();

        protected override void BuildConstructors(StringBuilder sb)
        {
        }

        protected override void BuildFields(StringBuilder sb)
        {
            bool isFirst = true;
            foreach(EnumValue value in Values)
            {
                if (isFirst) isFirst = false;
                else sb.Append(',');

                value.Build(sb);
            }
        }

        protected override void BuildMethods(StringBuilder sb)
        {
        }

        protected override void BuildProperties(StringBuilder sb)
        {
        }

        protected override void BuildGenericConstraints(StringBuilder sb)
        {
        }
    }
}