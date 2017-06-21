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

        internal void Initialize( string name )
        {
            Debug.Assert( _name == null );
            _name = name;
        }
    }
}
