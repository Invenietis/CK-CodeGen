using System;
using FluentAssertions;
using NUnit.Framework;

namespace CK.CodeGen.Abstractions.Tests
{
    public abstract class NamespaceScopeTests : CodeScopeTests
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
            INamespaceScope sut = CreateGlobalNamespace();
            INamespaceScope ns = sut.FindOrCreateNamespace( "X.Y.Z" );

            ns.Parent.Should().BeSameAs( sut );
            ns.FullName.Should().Be( "X.Y.Z" );
            ns.Name.Should().Be( "Z" );
            ns.Namespaces.Should().BeEmpty();
            ns.Types.Should().BeEmpty();
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
            INamespaceScope sut = CreateGlobalNamespace();
            INamespaceScope namespace1 = sut.FindOrCreateNamespace( "X.Y" );
            INamespaceScope namespace2 = sut.FindOrCreateNamespace( "X.Y" );

            namespace1.Should().BeSameAs( namespace2 );
        }

        protected abstract INamespaceScope CreateGlobalNamespace();

        protected override ICodeScope CreateCodeScope()
        {
            INamespaceScope global = CreateGlobalNamespace();
            return global.FindOrCreateNamespace( "X.Y.Z" );
        }
    }
}
