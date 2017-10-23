using System;
using FluentAssertions;
using NUnit.Framework;
using System.Linq;
using CK.Text;

namespace CK.CodeGen.Abstractions.Tests
{
    public abstract class NamespaceScopeTests : TypeDefinerScopeTests
    {
        [Test]
        public void initialize_empty_global_namespace()
        {
            INamespaceScope sut = CreateWorkspace().Global;

            sut.Parent.Should().BeNull();
            sut.FullName.Should().BeEmpty();
            sut.Name.Should().BeEmpty();
            sut.Namespaces.Should().BeEmpty();
            sut.Types.Should().BeEmpty();
        }

        [Test]
        public void initialize_named_namespace()
        {
            INamespaceScope sut = CreateWorkspace().Global;
            INamespaceScope ns3 = sut.FindOrCreateNamespace( "X.Y.Z" );
            INamespaceScope ns2 = ns3.Parent;
            INamespaceScope ns1 = ns2.Parent;

            ns3.FullName.Should().Be( "X.Y.Z" );
            ns3.Name.Should().Be( "Z" );
            ns3.Namespaces.Should().BeEmpty();
            ns3.Types.Should().BeEmpty();

            ns2.FullName.Should().Be( "X.Y" );
            ns2.Name.Should().Be( "Y" );
            ns2.Namespaces.Should().ContainSingle( x => x == ns3 );
            ns2.Types.Should().BeEmpty();

            ns1.FullName.Should().Be( "X" );
            ns1.Name.Should().Be( "X" );
            ns1.Namespaces.Should().ContainSingle( x => x == ns2 );
            ns1.Types.Should().BeEmpty();

            sut.Namespaces.Should().ContainSingle( x => x == ns1 );
            sut.Types.Should().BeEmpty();
        }

        [Test]
        public void initialize_subnamespace()
        {
            INamespaceScope global = CreateWorkspace().Global;
            INamespaceScope sut = global.FindOrCreateNamespace( "X.Y" );
            INamespaceScope nested = sut.FindOrCreateNamespace( "Z" );

            sut.Namespaces.Should().BeEquivalentTo( nested );
            nested.Parent.Should().BeSameAs( sut );
            nested.FullName.Should().Be( "X.Y.Z" );
            nested.Name.Should().Be( "Z" );
            global.Namespaces.Select( ns => ns.FullName ).Single().Should().Be( "X" );
        }

        [Test]
        public void find_existing_namespace()
        {
            INamespaceScope sut = CreateWorkspace().Global;
            INamespaceScope namespace1 = sut.FindOrCreateNamespace( "X.Y" );
            INamespaceScope namespace2 = sut.FindOrCreateNamespace( "X.Y" );

            namespace1.Should().BeSameAs( namespace2 );
        }

        protected abstract ICodeWorkspace CreateWorkspace();

        protected override ITypeDefinerScope CreateTypeDefinerScope()
        {
            INamespaceScope global = CreateWorkspace().Global;
            return global.FindOrCreateNamespace( "X.Y.Z" );
        }
    }
}
