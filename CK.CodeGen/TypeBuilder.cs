﻿using CK.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen
{
    public abstract class TypeBuilder
    {
        readonly NamespaceBuilder _namespace;
        readonly string _kind;

        internal TypeBuilder(NamespaceBuilder namespaceBuilder, string kind, string name)
        {
            _namespace = namespaceBuilder;
            _kind = kind;
            Name = name;
        }

        public NamespaceBuilder NamespaceBuilder => _namespace;

        public List<string> Attributes { get; } = new List<string>();

        public List<string> FrontModifiers { get; } = new List<string>();

        public string Name { get; set; }

        public string FullName => $"{_namespace.Name}.{Name}";

        protected abstract IReadOnlyCollection<string> Parents { get; }

        public StringBuilder ExtraBody { get; } = new StringBuilder();

        internal void Build(StringBuilder b)
        {
            BuildHelpers.BuildAttributes(b, Attributes);
            BuildHelpers.BuildFrontModifiers(b, FrontModifiers);
            BuildKind(b);
            BuildName(b);
            BuildParents(b);
            b.AppendLine();
            BuildGenericConstraints(b);
            b.AppendLine("{");
            BuildFields(b);
            BuildConstructors(b);
            BuildProperties(b);
            BuildMethods(b);
            BuildExtraBody(b);
            b.AppendLine("}");
        }

        void BuildKind(StringBuilder b)
        {
            b.AppendWithWhitespace(_kind);
        }

        void BuildName(StringBuilder b)
        {
            b.Append(Name);
        }

        void BuildParents(StringBuilder b)
        {
            if (HasParents)
            {
                b.Append(":").AppendStrings(Parents);
            }
        }

        protected abstract void BuildGenericConstraints(StringBuilder sb);

        protected abstract void BuildFields(StringBuilder sb);

        protected abstract void BuildConstructors(StringBuilder sb);

        protected abstract void BuildProperties(StringBuilder sb);

        protected abstract void BuildMethods(StringBuilder sb);

        void BuildExtraBody(StringBuilder sb)
        {
            if (HasExtraBody)
            {
                sb.Append(ExtraBody.ToString());
            }
        }

        bool HasExtraBody => ExtraBody.Length > 0;

        bool HasParents => Parents.Count > 0;
    }
}