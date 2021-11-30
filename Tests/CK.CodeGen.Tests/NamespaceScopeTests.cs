using System;
using CK.CodeGen;
using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using System.Text.RegularExpressions;
using System.Linq;

namespace CK.CodeGen.Tests
{
    [TestFixture]
    public class NamespaceScopeTests : CommonTypeDefinerScopeTests
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

            sut.Namespaces.Should().BeEquivalentTo( new[] { nested } );
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

        [Test]
        public void using_simple_optimization()
        {
            INamespaceScope global = CreateWorkspace().Global;
            INamespaceScope nAXa = global.FindOrCreateNamespace( "A.X.a" );
            INamespaceScope nAXb = global.FindOrCreateNamespace( "A.X.b" );
            INamespaceScope nAX = global.FindOrCreateNamespace( "A.X" );
            INamespaceScope nA = global.FindOrCreateNamespace( "A" );

            global.EnsureUsing( "EveryWhere" );

            nAXa.EnsureUsing( "EveryWhere" );
            nAX.EnsureUsing( "EveryWhere" );
            nA.EnsureUsing( "EveryWhere" );
            nAXb.EnsureUsing( "EveryWhere" );
            ExtractNamespaces( nAXa ).Should().BeEmpty();
            ExtractNamespaces( nAXb ).Should().BeEmpty();
            ExtractNamespaces( nAX ).Should().BeEmpty();
            ExtractNamespaces( nA ).Should().BeEmpty();
            ExtractNamespaces( global ).Should().OnlyContain( x => x == "EveryWhere" );

            nAXb.EnsureUsing( "Local" );
            ExtractNamespaces( nAXa ).Should().BeEmpty();
            ExtractNamespaces( nAXb ).Should().OnlyContain( x => x == "Local" );
            ExtractNamespaces( nAX ).Should().BeEmpty();
            ExtractNamespaces( nA ).Should().BeEmpty();
            ExtractNamespaces( global ).Should().OnlyContain( x => x == "EveryWhere" );

            nAXa.EnsureUsing( "Local" );
            ExtractNamespaces( nAXa ).Should().OnlyContain( x => x == "Local" );
            ExtractNamespaces( nAXb ).Should().OnlyContain( x => x == "Local" );
            ExtractNamespaces( nAX ).Should().BeEmpty();
            ExtractNamespaces( nA ).Should().BeEmpty();
            ExtractNamespaces( global ).Should().OnlyContain( x => x == "EveryWhere" );
        }

        [Test]
        public void using_alias_are_a_bit_normalized_by_removing_white_spaces()
        {
            ICodeWorkspace workspace = CreateWorkspace();
            INamespaceScope global = workspace.Global;
            INamespaceScope nAXa = global.FindOrCreateNamespace( "A.X.a" );
            INamespaceScope nAXb = global.FindOrCreateNamespace( "A.X.b" );
            INamespaceScope nAX = global.FindOrCreateNamespace( "A.X" );
            INamespaceScope nA = global.FindOrCreateNamespace( "A" );

            global.EnsureUsingAlias( "INT", "System.UInt8" );
            nAXa.EnsureUsingAlias( "INT", "System . UInt8" );

            ExtractNamespaces( global ).Should().OnlyContain( x => x == "INT" );
            ExtractNamespaces( nAXa ).Should().BeEmpty();

            var source = workspace.GetGlobalSource();
            Normalize( source ).Should().Be( Normalize(
                @"namespace A
{
using INT = System.UInt8;
namespace X
    {
        namespace a { }
        namespace b { }
    }
}"
                ) );

            global.EnsureUsingAlias( "CNode", "SNode<string,bool>" );
            nAXa.EnsureUsingAlias( "CNode", "SNode<STRING, BOOL>" );
            nAXa.Append( "// The 2 CNode definition differ." ).NewLine();

            ExtractNamespaces( global ).Should().HaveCount(2).And.OnlyContain( x => x == "INT" || x == "CNode" );
            ExtractNamespaces( nAXa ).Should().OnlyContain( x => x == "CNode" );

            global.FindOrCreateNamespace( "ToShowTheTopLevelUsingsCopy" );
            source = workspace.GetGlobalSource();
            Normalize( source ).Should().Be( Normalize(
                @"namespace A {
                    using INT = System.UInt8;
                    using CNode = SNode<string,bool>;
                    namespace X {
                      namespace a {
                        using CNode = SNode<STRING, BOOL>;
                        // The 2 CNode definition differ.
                      }
                      namespace b {
                      }
                    }
                  }
namespace ToShowTheTopLevelUsingsCopy
{
                    using INT = System.UInt8;
                    using CNode = SNode<string,bool>;
}"
                ) );
        }

        static IReadOnlyCollection<string> ExtractNamespaces( INamespaceScope n )
        {
            var d = (Dictionary<string, KeyValuePair<string, string>>)n.GetType().GetField( "_usings", BindingFlags.NonPublic | BindingFlags.Instance ).GetValue( n );
            return d.Keys;
        }

        static string Normalize( string s )
        {
            return Regex.Replace( s, @"\s+", "|" );
        }

        ICodeWorkspace CreateWorkspace() => CodeWorkspace.Create();

        protected override ITypeDefinerScope CreateTypeDefinerScope()
        {
            return CreateWorkspace().Global.FindOrCreateNamespace( "NS.ForCommon" );
        }
    }
}
