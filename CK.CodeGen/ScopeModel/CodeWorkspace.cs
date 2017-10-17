using System.Reflection;
using CK.CodeGen.Abstractions;
using System.Collections.Generic;
using System;

namespace CK.CodeGen
{
    /// <summary>
    /// This class exposes the main entry point of the source code model.
    /// </summary>
    public class CodeWorkspace : ICodeWorkspace
    {
        readonly HashSet<Assembly> _assemblies;

        /// <summary>
        /// Creates a root workspace.
        /// </summary>
        /// <returns>A new empty worspace.</returns>
        public static ICodeWorkspace Create() => new CodeWorkspace();

        CodeWorkspace()
        {
            _assemblies = new HashSet<Assembly>();
            Global = new NamespaceScopeImpl( this, null, String.Empty );
        }

        INamespaceScope ICodeWorkspace.Global => Global;

        internal NamespaceScopeImpl Global { get; }

        public ICodeWorkspace EnsureAssemblyReference( Assembly assembly )
        {
            _assemblies.Add( assembly );
            return this;
        }
    }
}
