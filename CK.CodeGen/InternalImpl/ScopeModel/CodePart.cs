using CK.CodeGen;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen;

class CodePart : BaseCodePart, INamedScope
{
    readonly INamedScope _owner;

    public CodePart( INamedScope owner )
    {
        _owner = owner;
    }

    public INamedScope PartOwner => _owner;

    ICodeWorkspace INamedScope.Workspace => _owner.Workspace;

    INamedScope? INamedScope.Parent => _owner.Parent;

    string INamedScope.Name => _owner.Name;

    string INamedScope.FullName => _owner.FullName;

    void INamedScope.Build( Action<string> collector, bool closeScope ) => _owner.Build( collector, closeScope );

    public override string ToString() => Build( new SmarterStringBuilder( new StringBuilder() ) ).ToString();
}
