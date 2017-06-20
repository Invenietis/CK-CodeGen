using System;
using System.Collections.Generic;
using System.Text;
using CK.CodeGen.Abstractions;

namespace CK.CodeGen
{
    abstract class CodeScopeImpl : ICodeScope
    {
        protected CodeScopeImpl( ICodeScope parent )
        {
            Parent = parent;
            Builder = new StringBuilder();
        }

        public StringBuilder Builder { get; }

        public ICodeScope Parent { get; }

        public abstract string Name { get; }

        protected abstract string LocalName { get; }

        public string FullName => Parent != null ? string.Format( "{0}.{1}", Parent.FullName, LocalName ) : LocalName;

        public ITypeScope CreateType( Action<ICodeScope> header )
        {
            throw new NotImplementedException();
        }

        public ITypeScope FindType( string name )
        {
            throw new NotImplementedException();
        }

        public abstract IReadOnlyList<ITypeScope> Types { get; }

        public abstract IReadOnlyList<string> Usings { get; }

        public abstract void AddUsing( string ns );
    }
}