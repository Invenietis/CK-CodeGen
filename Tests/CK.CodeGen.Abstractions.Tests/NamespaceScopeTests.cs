using FluentAssertions;
using NUnit.Framework;

namespace CK.CodeGen.Abstractions.Tests
{
    public abstract class NamespaceScopeTests
    {
        [Test]
        public void initialize_global_namespace()
        {
            INamespaceScope sut = CreateGlobalNamespace();

            sut.Parent.Should().BeNull();
            sut.FullName.Should().BeEmpty();
            sut.Name.Should().BeEmpty();
            sut.Namespaces.Should().BeEmpty();
            sut.Types.Should().BeEmpty();
            sut.Usings.Should().BeEmpty();
        }

        protected abstract INamespaceScope CreateGlobalNamespace();
    }
}
