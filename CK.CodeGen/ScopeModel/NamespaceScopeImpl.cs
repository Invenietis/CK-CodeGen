using System;
using System.Collections.Generic;
using CK.CodeGen.Abstractions;

namespace CK.CodeGen
{
    sealed class NamespaceScopeImpl : CodeScopeImpl, INamespaceScope
    {
        readonly Dictionary<string, NamespaceScopeImpl> _namespaces;

        internal NamespaceScopeImpl( INamespaceScope parent, string ns )
            : base( parent )
        {
            if( ns == null ) ns = string.Empty;
            _namespaces = new Dictionary<string, NamespaceScopeImpl>();
            LocalName = ns;
            string[] parts = ns.Split( '.' );
            Name = parts[parts.Length - 1];
        }

        public override string Name { get; }

        protected override string LocalName { get; }

        public override IReadOnlyList<ITypeScope> Types => new ITypeScope[0];

        INamespaceScope INamespaceScope.Parent => (INamespaceScope)Parent;

        public override void AddUsing( string ns )
        {
            throw new NotImplementedException();
        }

        public override IReadOnlyList<string> Usings => new string[0];

        public INamespaceScope FindOrCreateNamespace( string ns )
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<INamespaceScope> Namespaces => _namespaces.Values;
    }
}
