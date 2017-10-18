using System.Reflection;
using CK.CodeGen.Abstractions;
using System.Collections.Generic;
using System;

namespace CK.CodeGen
{
    class CodeWorkspaceImpl : ICodeWorkspace
    {
        readonly HashSet<Assembly> _assemblies;

        public CodeWorkspaceImpl()
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
