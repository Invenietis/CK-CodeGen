using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CK.CodeGen.Tests
{
    public class DefaultAssemblyResolver : CK.CodeGen.IAssemblyResolver
    {
        static public readonly DefaultAssemblyResolver Default = new DefaultAssemblyResolver();

        public string GetPath(Assembly a) => a.Location;

        public IEnumerable<AssemblyName> GetReferencedAssemblies(Assembly a) => a.GetReferencedAssemblies();

        public Assembly LoadByName(AssemblyName n) => Assembly.Load(n);
    }
}
