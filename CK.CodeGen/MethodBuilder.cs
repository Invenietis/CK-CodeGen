using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen
{
    public class MethodBuilder : MethodBaseBuilder
    {
        internal MethodBuilder(ClassBuilder classBuilder, string frontModifiers, string name)
            : base(classBuilder, name)
        {
            FrontModifiers = new List<string>();
            if (frontModifiers != null) FrontModifiers.AddRange(frontModifiers.Split(' '));
        }

        public List<string> FrontModifiers { get; }

        public StringBuilder Body { get; } = new StringBuilder();

        protected override void BuildFrontModifiers(StringBuilder sb)
        {
            BuildHelpers.BuildFrontModifiers(FrontModifiers, sb);
        }

        internal override void BuildBody(StringBuilder sb)
        {
            if (IsAbstract) sb.Append(";");
            else BuildHelpers.BuildMethodBody(Body.ToString(), sb);
        }

        bool IsAbstract => FrontModifiers.Contains("abstract");
    }
}