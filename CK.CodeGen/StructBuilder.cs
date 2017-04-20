namespace CK.CodeGen
{
    public class StructBuilder : ClassBuilder
    {
        internal StructBuilder(NamespaceBuilder namespaceBuilder, string name)
            : base(namespaceBuilder, "struct", name)
        {
        }
    }
}
