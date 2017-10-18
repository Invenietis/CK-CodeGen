using CK.CodeGen.Abstractions;
using NUnit.Framework;

namespace CK.CodeGen.Tests
{
    [TestFixture]
    public class NamespaceScopeTests : Abstractions.Tests.NamespaceScopeTests
    {
        protected override ICodeWorkspace CreateWorkspace() => CodeWorkspace.Create();
    }
}
