using CK.CodeGen.Abstractions;
using NUnit.Framework;

namespace CK.CodeGen.Tests
{
    [TestFixture]
    public class TypeScopeTests : Abstractions.Tests.TypeScopeTests
    {
        protected override INamespaceScope CreateGlobalNamespace() => CodeScope.CreateGlobalNamespace();
    }
}
