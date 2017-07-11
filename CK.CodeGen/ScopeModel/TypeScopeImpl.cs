using System;
using System.Collections.Generic;
using System.Diagnostics;
using CK.CodeGen.Abstractions;

namespace CK.CodeGen
{
    sealed class TypeScopeImpl : CodeScopeImpl, ITypeScope
    {
        string _name;

        internal TypeScopeImpl( ICodeScope parent )
            : base( parent )
        {
        }

        public override string Name
        {
            get
            {
                Debug.Assert( !string.IsNullOrWhiteSpace( _name ) );
                return _name;
            }
        }

        protected override string LocalName => Name;

        public override void EnsureUsing( string ns )
        {
            Parent.EnsureUsing( ns );
        }

        public override void EnsurePackageReference( string name, string version )
        {
            Parent.EnsurePackageReference( name, version );
        }

        public override void EnsureAssemblyReference( string name, string version )
        {
            Parent.EnsureAssemblyReference( name, version );
        }

        internal void Initialize( string name )
        {
            Debug.Assert( _name == null );
            _name = name;
        }
    }
}
