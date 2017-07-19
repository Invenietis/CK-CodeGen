using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using CK.CodeGen.Abstractions;

namespace CK.CodeGen
{
    sealed class NamespaceScopeImpl : CodeScopeImpl, INamespaceScope
    {
        readonly Dictionary<string, NamespaceScopeImpl> _namespaces;
        readonly HashSet<string> _usings;
        readonly HashSet<VersionedReference> _packageReferences;
        readonly HashSet<Assembly> _assemblies;

        internal NamespaceScopeImpl( INamespaceScope parent, string ns )
            : base( parent )
        {
            if( ns == null ) ns = string.Empty;
            _namespaces = new Dictionary<string, NamespaceScopeImpl>();
            _usings = new HashSet<string>();
            _packageReferences = new HashSet<VersionedReference>();
            _assemblies = new HashSet<Assembly>();
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

        public override void EnsureAssemblyReference( Assembly assembly )
        {
            _assemblies.Add( assembly );
        }

        public override string Build( bool close )
        {
            StringBuilder sb = new StringBuilder();
            if( !IsGlobal ) sb.AppendFormat( "namespace {0} {{", Name ).AppendLine();

            foreach( string u in _usings ) sb.AppendFormat( "using {0};", u ).AppendLine();
            foreach( TypeScopeImpl type in Types )
            {
                type.Build( sb, true );
                sb.AppendLine();
            }

            if( !IsGlobal && close ) sb.AppendLine( "}" );
            return sb.ToString();
        }

        public bool IsGlobal => Parent == null;

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
