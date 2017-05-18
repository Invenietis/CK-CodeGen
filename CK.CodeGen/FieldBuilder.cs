using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CK.CodeGen
{
    public class FieldBuilder
    {
        readonly ClassBuilder _ClassBuilder;

        internal FieldBuilder(ClassBuilder classBuilder, string type, string name)
        {
            _ClassBuilder = classBuilder;
            Type = type;
            Name = name;
        }

        public List<string> Attributes { get; } = new List<string>();

        public List<string> FrontModifiers { get; } = new List<string>();

        public string Type { get; set; }

        public string Name { get; set; }

        public string InitialValue { get; set; }

        internal void Build(StringBuilder sb)
        {
            BuildHelpers.BuildAttributes(sb, Attributes);
            BuildHelpers.BuildFrontModifiers(sb, FrontModifiers);
            sb.AppendFormat("{0} {1}", Type, Name);
            if (HasInitialValue) sb.AppendFormat("={0}", InitialValue);
            sb.AppendLine(";");
        }

        bool HasInitialValue => !string.IsNullOrEmpty(InitialValue);
    }
}