using CK.CodeGen.Abstractions;

namespace CK.CodeGen
{
    public static class CodeScope
    {
        public static INamespaceScope CreateNamespace( string ns ) => new NamespaceScopeImpl( null, ns );
    }
}
