using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen
{
    public abstract class PropertyBaseBuilder
    {
        readonly TypeBuilder _type;

        protected PropertyBaseBuilder(TypeBuilder typeBuilder, string type, string name)
        {
            _type = typeBuilder;
            Type = type;
            Name = name;
        }

        public List<string> Attributes { get; } = new List<string>();

        public List<string> FrontModifiers { get; } = new List<string>();

        public string Type { get; set; }

        public string Name { get; set; }

        internal void Build(StringBuilder sb)
        {
            BuildHelpers.BuildAttributes(sb, Attributes);
            BuildHelpers.BuildFrontModifiers(sb, FrontModifiers);
            BuildType(sb);
            BuildName(sb);
            BuildMethods(sb);
        }

        void BuildType(StringBuilder sb)
        {
            sb.AppendWithWhitespace(Type);
        }

        void BuildName(StringBuilder sb)
        {
            sb.Append(Name);
        }

        internal abstract void BuildMethods(StringBuilder sb);
    }
}