using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CK.CodeGen.Tests
{
    public class DefaultAssemblyResolver : IAssemblyResolver
    {
        static public readonly DefaultAssemblyResolver Default = new DefaultAssemblyResolver();

        public string GetAssemblyFilePath( Assembly a ) => new Uri( a.CodeBase ).LocalPath;

        public IEnumerable<AssemblyName> GetReferencedAssemblies( Assembly a ) => a.GetReferencedAssemblies();

        public Assembly LoadByName( AssemblyName n ) => Assembly.Load( n );
    }
}
