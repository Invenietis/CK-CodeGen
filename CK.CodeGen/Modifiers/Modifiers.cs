using CK.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CK.CodeGen.Modifiers
{
    public struct List<T>
    {
        public List(IList<T> list) => Target = list;
        public IList<T> Target { get; }

        public List<T> Add(T val)
        {
            Target.Add(val);
            return this;
        }
        public List<T> Remove(T val)
        {
            Target.Remove(val);
            return this;
        }
        public List<T> Clear()
        {
            Target.Clear();
            return this;
        }
    }

    public struct ClassBuilder
    {
        public ClassBuilder(CK.CodeGen.ClassBuilder c) => Target = c;
        public CK.CodeGen.ClassBuilder Target { get; }

        public ClassBuilder SetBase(Type baseType, params string[] genericParams)
        {
            Target.ActualBaseType = new BaseType(baseType, genericParams);
            return this;
        }

        public ClassBuilder SetBase(string baseType)
        {
            Target.BaseType = baseType;
            return this;
        }

        public ClassBuilder DefineOverrideMethod(MethodInfo baseMethod, Action<StringBuilder> bodyBuilder = null)
        {
            if (baseMethod == null) throw new ArgumentNullException(nameof(baseMethod));
            string name = baseMethod.Name;
            if (baseMethod.ContainsGenericParameters)
            {
                name += '<';
                name += baseMethod.GetGenericArguments().Select(a => a.Name).Concatenate();
                name += '>';
            }
            MethodBuilder mB = new MethodBuilder(Target.DefineMethod(name));
            mB.Target.BaseMethod = baseMethod;
            if (baseMethod.IsPublic) mB.Target.FrontModifiers.Add("public");
            else if (baseMethod.IsFamily) mB.Target.FrontModifiers.Add("protected");
            else if (baseMethod.IsFamilyAndAssembly)
            {
                // This is invalid since we are in another assembly!
                mB.Target.FrontModifiers.Add("private protected");
            }
            else if (baseMethod.IsFamilyOrAssembly)
            {
                // Should be "internal protected" but since we are in another assembly,
                // "internal" changes the protection. 
                mB.Target.FrontModifiers.Add("protected");
            }
            mB.Target.FrontModifiers.Add("override");
            mB.Target.ReturnType = baseMethod.ReturnType.CompleteName();
            foreach (var p in baseMethod.GetParameters())
            {
                mB.AddParameter(p);
            }
            bodyBuilder?.Invoke(mB.Target.Body);
            return this;
        }
    }

    public struct MethodBuilder
    {
        public MethodBuilder(CK.CodeGen.MethodBuilder c) => Target = c;

        public CK.CodeGen.MethodBuilder Target { get; }

        public MethodBuilder Body(Action<StringBuilder> c)
        {
            c(Target.Body);
            return this;
        }

        public MethodBuilder AddParameter(ParameterInfo p)
        {
            var pB = new ParameterBuilder();
            pB.Name = p.Name;
            if (p.IsOut) pB.Attributes.Add("out");
            else if (p.ParameterType.IsByRef) pB.Attributes.Add("ref");
            pB.ParameterType = p.ParameterType.IsByRef
                                ? p.ParameterType.GetElementType().CompleteName()
                                : p.ParameterType.CompleteName();
            Target.Parameters.Add(pB);
            return this;
        }
    }

}
