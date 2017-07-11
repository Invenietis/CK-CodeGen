using System;
using System.Collections.Generic;
using CK.CodeGen.Abstractions;

namespace CK.CodeGen
{
    sealed class NamespaceScopeImpl : CodeScopeImpl, INamespaceScope
    {
        readonly Dictionary<string, NamespaceScopeImpl> _namespaces;
        readonly HashSet<string> _usings;
        readonly HashSet<VersionedReference> _packageReferences;
        readonly HashSet<VersionedReference> _assemblyReferences;

        internal NamespaceScopeImpl( INamespaceScope parent, string ns )
            : base( parent )
        {
            if( ns == null ) ns = string.Empty;
            _namespaces = new Dictionary<string, NamespaceScopeImpl>();
            _usings = new HashSet<string>();
            _packageReferences = new HashSet<VersionedReference>();
            _assemblyReferences = new HashSet<VersionedReference>();
            LocalName = ns;
            string[] parts = ns.Split( '.' );
            Name = parts[parts.Length - 1];
        }

        public override string Name { get; }

        protected override string LocalName { get; }

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

        public override void EnsurePackageReference( string name, string version )
        {
            _packageReferences.Add( new VersionedReference( name, version ) );
        }

        public override void EnsureAssemblyReference( string name, string version )
        {
            _assemblyReferences.Add( new VersionedReference( name, version ) );
        }

        class VersionedReference
        {
            internal VersionedReference( string name, string version )
            {
                Name = name;
                Version = version;
            }

            internal readonly string Name;

            internal readonly string Version;

            public override bool Equals( object obj )
            {
                VersionedReference other = obj as VersionedReference;
                return other != null
                    && other.Name == Name
                    && other.Version == Version;
            }

            public override int GetHashCode()
            {
                return Name.GetHashCode() << 7 ^ Version.GetHashCode();
            }
        }
    }
}
