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
        }

        [Test]
        public void initialize_named_namespace()
        {
            INamespaceScope global = CreateGlobalNamespace();
            INamespaceScope sut = global.FindOrCreateNamespace( "X.Y.Z" );
            
            sut.Parent.Should().BeSameAs( global );
            sut.FullName.Should().Be( "X.Y.Z" );
            sut.Name.Should().Be( "Z" );
            sut.Namespaces.Should().BeEmpty();
            sut.Types.Should().BeEmpty();
        }

        [Test]
        public void initialize_subnamespace()
        {
            INamespaceScope global = CreateGlobalNamespace();
            INamespaceScope sut = global.FindOrCreateNamespace( "X.Y" );
            INamespaceScope nested = sut.FindOrCreateNamespace( "Z" );

            sut.Namespaces.Should().BeEquivalentTo( nested );
            nested.Parent.Should().BeSameAs( sut );
            nested.FullName.Should().Be( "X.Y.Z" );
            nested.Name.Should().Be( "Z" );
        }

        [Test]
        public void find_existing_namespace()
        {
            INamespaceScope global = CreateGlobalNamespace();
            INamespaceScope sut = global.FindOrCreateNamespace( "X.Y" );
            INamespaceScope ns = global.FindOrCreateNamespace( "X.Y" );

            sut.Should().BeSameAs( ns );
        }

        protected abstract INamespaceScope CreateGlobalNamespace();
    }
}
