using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen
{
    public abstract class MethodBaseBuilder
    {
        readonly TypeBuilder _type;

        internal MethodBaseBuilder(TypeBuilder typeBuilder, string name)
        {
            _type = typeBuilder;
            Name = name;
        }

        public List<string> Attributes { get; } = new List<string>();

        public string Name { get; set; }

        public string ReturnType { get; set; }

        public List<Parameter> Parameters { get; } = new List<Parameter>();

        public List<GenericConstraint> GenericConstraints { get; set; } = new List<GenericConstraint>();

        internal void Build(StringBuilder sb)
        {
            BuildHelpers.BuildAttributes(sb, Attributes);
            BuildFrontModifiers(sb);
            BuildReturnType(sb);
            BuildName(sb);
            BuildHelpers.BuildParameters(sb, Parameters);
            BuildGenericConstraints(sb);
            BuildBody(sb);
        }

        protected abstract void BuildFrontModifiers(StringBuilder sb);

        void BuildReturnType(StringBuilder sb)
        {
            if (IsVoid) sb.AppendWithWhitespace("void");
            else sb.AppendWithWhitespace(ReturnType);
        }

        void BuildName(StringBuilder sb)
        {
            sb.Append(Name);
        }

        void BuildGenericConstraints(StringBuilder sb)
        {
            if (HasGenericConstraint)
            {
                foreach (GenericConstraint constraint in GenericConstraints)
                {
                    sb.AppendFormat("where {0}:", constraint.GenericParameterName);
                    sb.Append(string.Join(",", constraint.Constraints));
                }
            }
        }

        internal abstract void BuildBody(StringBuilder sb);

        bool HasGenericConstraint => GenericConstraints.Count > 0;

        bool IsVoid => string.IsNullOrEmpty(ReturnType);
    }
}
