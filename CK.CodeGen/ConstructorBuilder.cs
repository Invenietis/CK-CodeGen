using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace CK.CodeGen
{
    public class ConstructorBuilder
    {
        readonly ClassBuilder _typeBuilder;

        internal ConstructorBuilder(ClassBuilder classBuilder)
        {
            _typeBuilder = classBuilder;
        }

        public List<string> Attributes { get; } = new List<string>();

        public List<string> FrontModifiers { get; } = new List<string>();

        public List<ParameterBuilder> Parameters { get; } = new List<ParameterBuilder>();

        public string Initializer { get; set; }

        public StringBuilder Body { get; } = new StringBuilder();

        internal void Build(StringBuilder sb)
        {
            BuildHelpers.BuildAttributes(sb, Attributes);
            BuildHelpers.BuildFrontModifiers(sb, FrontModifiers);
            BuildName(sb);
            BuildHelpers.BuildParameters(sb, Parameters);
            BuildInitializer(sb);
            BuildBody(sb);
        }

        void BuildName(StringBuilder sb)
        {
            string name = Regex.Replace(_typeBuilder.Name, @"(?<1>\w+)(?<2>\s*<[^>]+>)?", "${1}", RegexOptions.ExplicitCapture);
            sb.Append(name);
        }

        void BuildInitializer(StringBuilder sb)
        {
            if(HasInitializer)
            {
                sb.AppendFormat(":{0}", Initializer);
            }
        }

        void BuildBody(StringBuilder sb)
        {
            sb.Append("{");
            sb.Append(Body.ToString());
            sb.AppendLine("}");
        }

        bool HasInitializer => !string.IsNullOrEmpty(Initializer);
    }
}