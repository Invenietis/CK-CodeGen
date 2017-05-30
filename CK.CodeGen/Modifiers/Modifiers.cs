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

        public ClassBuilder SetBase(Type baseType)
        {
            Target.ActualBaseType = new BaseType(baseType);
            return this;
        }

        public ClassBuilder SetBase(string baseType)
        {
            Target.BaseType = baseType;
            return this;
        }

        public ClassBuilder AddFrontModifiers(params string[] frontModifiers)
        {
            Target.FrontModifiers.AddRange( frontModifiers );
            return this;
        }

        /// <summary>
        /// Creates constructors that relay calls to public and protected constructors in the base class.
        /// <see cref="SetBase(Type)"/> must have been called before.
        /// </summary> 
        /// <param name="baseConstructorfilter">
        /// Optional predicate used to filter constructors that must be implemented.
        /// When null, all public and protected constructors are public.
        /// Returning a null string prevents implementation, otherwise the string is the front modifier of the constructor.
        /// </param>
        public ClassBuilder DefinePassThroughConstructors( Func<ConstructorInfo, string> baseConstructorfilter = null)
        {
            foreach (var baseCtor in Target.ActualBaseType.Type.GetTypeInfo().DeclaredConstructors.Where(c => c.IsPublic || c.IsFamily || c.IsFamilyOrAssembly))
            {
                string frontModifiers = "public";
                if (baseConstructorfilter != null && (frontModifiers = baseConstructorfilter(baseCtor)) == null) continue;
                var parameters = baseCtor.GetParameters();
                if (parameters.Length == 0) Target.DefineConstructor(frontModifiers);
                else
                {
                    ConstructorBuilder ccB = Target.DefineConstructor(frontModifiers);
                    ccB.Parameters.AddParameters(parameters);
                    ccB.Initializer = "base(" + ccB.Parameters.Select(p => p.Name).Concatenate() + ")";
                }
            }
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
            MethodBuilder mB = Target.DefineMethod(name);
            mB.BaseMethod = baseMethod;
            ModifierHelper.AddFrontModifiersProtection(baseMethod, mB.FrontModifiers);
            mB.FrontModifiers.Add("override");
            mB.ReturnType = baseMethod.ReturnType.ToCSharpName( true );
            mB.Parameters.AddParameters(baseMethod);
            bodyBuilder?.Invoke(mB.Body);
            return this;
        }
    }

}
