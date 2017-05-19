using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CK.CodeGen
{

    public interface IAssemblyResolver
    {
        Assembly LoadByName(AssemblyName n);

        IEnumerable<AssemblyName> GetReferencedAssemblies(Assembly a);

        string GetPath(Assembly a);
    }
}
