using CK.CodeGen.Abstractions;

namespace CK.CodeGen
{
    public static class CodeScope
    {
        public static INamespaceScope CreateGlobalNamespace() => new NamespaceScopeImpl( null, string.Empty );
    }
}
