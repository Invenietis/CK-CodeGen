using System;
using System.Collections.Generic;
using CK.CodeGen.Abstractions;

namespace CK.CodeGen
{
    sealed class TypeScopeImpl : CodeScopeImpl, ITypeScope
    {
        internal TypeScopeImpl( ICodeScope parent )
            : base( parent )
        {
        }

        public override string Name => throw new NotImplementedException();

        protected override string LocalName => throw new NotImplementedException();

        public override IReadOnlyList<ITypeScope> Types => throw new NotImplementedException();

        public override IReadOnlyList<string> Usings => throw new NotImplementedException();

        public override void AddUsing( string ns )
        {
            throw new NotImplementedException();
        }
    }
}
