using FluentAssertions;
using NUnit.Framework;

namespace CK.CodeGen.Abstractions.Tests
{
    public abstract class NamespaceScopeTests
    {
        [Test]
        public void initialize_an_empty_namespace()
        {
            INamespaceScope sut = CreateNamespaceScope( "X.Y.Z" );

            sut.Parent.Should().BeNull();
            sut.FullName.Should().Be( "X.Y.Z" );
            sut.Name.Should().Be( "Z" );
            sut.Namespaces.Should().BeEmpty();
            sut.Types.Should().BeEmpty();
            sut.Usings.Should().BeEmpty();
        }

        protected abstract INamespaceScope CreateNamespaceScope( string ns );
    }
}
