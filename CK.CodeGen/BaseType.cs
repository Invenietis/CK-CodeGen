using System;
using System.Collections.Generic;
using System.Linq;
using CK.Text;

namespace CK.CodeGen
{
    public class BaseType
    {
        public BaseType(Type type, IReadOnlyCollection<string> genericParams)
        {
            Type = type;
            GenericParams = genericParams ?? new string[0];
        }

        public Type Type { get; }

        public IReadOnlyCollection<string> GenericParams { get; }

        internal string BuildFullName()
        {
            if (!HasGenericParam) return Type.FullName;
            return string.Format(
                "{0}<{1}>",
                Type.FullName.Split('`')[0],
                GenericParams.Concatenate());
        }

        bool HasGenericParam => GenericParams.Count > 0;

        public override bool Equals(object obj)
        {
            BaseType other = obj as BaseType;
            return other != null
                && other.Type == Type
                && other.GenericParams.Count == GenericParams.Count
                && other.GenericParams.Zip(GenericParams, (p1, p2) => new { p1, p2 }).All(x => x.p1 == x.p2);
        }

        public override int GetHashCode()
        {
            int hash = Type.GetHashCode();
            foreach (string p in GenericParams) hash = (hash << 7) ^ p.GetHashCode();
            return hash;
        }
    }
}
