using System.Collections.Generic;

namespace CK.CodeGen.Abstractions
{
    public interface INamespaceScope : ICodeScope
    {
        new INamespaceScope Parent { get; }

        INamespaceScope FindOrCreateNamespace( string ns );

        IReadOnlyCollection<INamespaceScope> Namespaces { get; }

        bool IsGlobal { get; }
    }
}
