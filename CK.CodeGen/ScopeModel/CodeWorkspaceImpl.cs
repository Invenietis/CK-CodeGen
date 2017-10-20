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

        public IReadOnlyCollection<Assembly> AssemblyReferences => _assemblies;

        public void DoEnsureAssemblyReference( Assembly assembly )
        {
            if( assembly == null ) throw new ArgumentNullException( nameof( assembly ) );
            _assemblies.Add( assembly );
        }

        public void MergeWith( ICodeWorkspace other )
        {
            if( other == null ) throw new ArgumentNullException( nameof( other ) );
            if( other != this )
            {
                foreach( var a in other.AssemblyReferences ) _assemblies.Add( a );
                Global.MergeWith( (NamespaceScopeImpl)other.Global );
            }
        }
    }
}
