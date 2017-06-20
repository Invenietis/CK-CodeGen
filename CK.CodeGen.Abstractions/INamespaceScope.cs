using System.Collections.Generic;

namespace CK.CodeGen.Abstractions
{
    public interface INamespaceScope : ICodeScope
    {
        new INamespaceScope Parent { get; }

        INamespaceScope CreateNamespace( string name );
    }
}
