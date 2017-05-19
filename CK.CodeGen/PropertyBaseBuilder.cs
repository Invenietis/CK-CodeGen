using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen
{
    public abstract class PropertyBaseBuilder : TypeMemberBuilder
    {
        protected PropertyBaseBuilder(TypeBuilder typeBuilder, string type, string name)
            : base( typeBuilder )
        {
            PropertyType = type;
            Name = name;
        }

        public new TypeBuilder TypeBuilder => base.TypeBuilder;

        public List<string> Attributes { get; } = new List<string>();

        public List<string> FrontModifiers { get; } = new List<string>();

        public string PropertyType { get; set; }

        public string Name { get; set; }

        internal void Build(StringBuilder b)
        {
            BuildHelpers.BuildAttributes(b, Attributes);
            BuildHelpers.BuildFrontModifiers(b, FrontModifiers);
            BuildType(b);
            BuildName(b);
            BuildMethods(b);
        }

        void BuildType(StringBuilder b)
        {
            b.AppendWithWhitespace(PropertyType);
        }

        void BuildName(StringBuilder b)
        {
            b.Append(Name);
        }

        internal abstract void BuildMethods(StringBuilder b);
    }
}