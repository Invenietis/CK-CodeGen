using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CK.CodeGen
{
    public class MethodBuilder : MethodBaseBuilder
    {
        internal MethodBuilder(ClassBuilder classBuilder, string frontModifiers, string name)
            : base(classBuilder, name)
        {
            FrontModifiers = new List<string>();
            if (frontModifiers != null) FrontModifiers.AddRange(frontModifiers.Split(BuildHelpers.OneSpace,StringSplitOptions.RemoveEmptyEntries));
        }

        public new ClassBuilder TypeBuilder => (ClassBuilder)base.TypeBuilder;


        /// <summary>
        /// Gets or sets a method info associated to this <see cref="MethodBuilder"/>.
        /// </summary>
        public MethodInfo BaseMethod { get; set; }

        public List<string> FrontModifiers { get; }

        public StringBuilder Body { get; } = new StringBuilder();

        protected override void BuildFrontModifiers(StringBuilder sb)
        {
            BuildHelpers.BuildFrontModifiers(sb, FrontModifiers);
        }

        internal override void BuildBody(StringBuilder sb)
        {
            if (IsAbstract) sb.Append(";");
            else BuildHelpers.BuildMethodBody(sb, Body.ToString());
        }

        bool IsAbstract => FrontModifiers.Contains("abstract");
    }
}