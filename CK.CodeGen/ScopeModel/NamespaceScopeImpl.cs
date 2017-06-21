﻿using System.Collections.Generic;
using CK.CodeGen.Abstractions;

namespace CK.CodeGen
{
    sealed class NamespaceScopeImpl : CodeScopeImpl, INamespaceScope
    {
        readonly Dictionary<string, NamespaceScopeImpl> _namespaces;
        readonly HashSet<string> _usings;

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

        public override void EnsureUsing( string ns )
        {
            _usings.Add( ns );
        }

        public INamespaceScope FindOrCreateNamespace( string ns )
        {
            NamespaceScopeImpl result;
            if( !_namespaces.TryGetValue( ns, out result ) )
            {
                _namespaces[ns] = result = new NamespaceScopeImpl( this, ns );
            }
            return result;
        }

        public IReadOnlyCollection<INamespaceScope> Namespaces => _namespaces.Values;
    }
}