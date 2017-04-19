using System;
using System.Collections.Generic;
using System.Reflection;

namespace CK.CodeGen
{
    public class ModuleBuilder
    {
        IReadOnlyList<TypeBuilder> _types;

        public IReadOnlyList<TypeBuilder> Types => _types;

        public TypeBuilder DefineType(string name, TypeAttributes attr, Type parent)
        {

        }
        public TypeBuilder DefineType(string name, TypeAttributes attr, TypeBuilder parent)
        {

        }

        public string CreateSource()
        {
            return "";
        }
    }
}
