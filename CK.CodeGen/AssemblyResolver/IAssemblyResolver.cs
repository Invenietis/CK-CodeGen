using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CK.CodeGen
{
    /// <summary>
    /// Platform independent abstraction of Assembly manipulation.
    /// </summary>
    public interface IAssemblyResolver
    {
        /// <summary>
        /// Loads the assembly by its name.
        /// </summary>
        /// <param name="n">The name.</param>
        /// <returns>The corresponding assembly.</returns>
        Assembly LoadByName(AssemblyName n);

        /// <summary>
        /// Gets the referenced assembly names of an Assembly.
        /// </summary>
        /// <param name="a">The assembly.</param>
        /// <returns>The set of dependent assemblies.</returns>
        IEnumerable<AssemblyName> GetReferencedAssemblies(Assembly a);

        /// <summary>
        /// Gets the path of an assembly.
        /// </summary>
        /// <param name="a">The assembly.</param>
        /// <returns>The assembly path.</returns>
        string GetAssemblyFilePath(Assembly a);
    }
}
