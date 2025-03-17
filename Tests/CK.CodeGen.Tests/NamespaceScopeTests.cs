using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;
using Shouldly;
using System.Text.RegularExpressions;
using System.Linq;
using CK.Core;

namespace CK.CodeGen.Tests;

[TestFixture]
public class NamespaceScopeTests : CommonTypeDefinerScopeTests
{
    [Test]
    public void initialize_empty_global_namespace()
    {
        INamespaceScope sut = CreateWorkspace().Global;

        sut.Parent.ShouldBeNull();
        sut.FullName.ShouldBeEmpty();
        sut.Name.ShouldBeEmpty();
        sut.Namespaces.ShouldBeEmpty();
        sut.Types.ShouldBeEmpty();
    }

    [Test]
    public void initialize_named_namespace()
    {
        INamespaceScope sut = CreateWorkspace().Global;
        INamespaceScope ns3 = sut.FindOrCreateNamespace( "X.Y.Z" );
        INamespaceScope? ns2 = ns3.Parent;
        ns2.ShouldNotBeNull();
        INamespaceScope? ns1 = ns2.Parent;
        ns1.ShouldNotBeNull();

        ns3.FullName.ShouldBe( "X.Y.Z" );
        ns3.Name.ShouldBe( "Z" );
        ns3.Namespaces.ShouldBeEmpty();
        ns3.Types.ShouldBeEmpty();

        ns2.FullName.ShouldBe( "X.Y" );
        ns2.Name.ShouldBe( "Y" );
        ns2.Namespaces.ShouldContain( ns3 );
        ns2.Types.ShouldBeEmpty();

        ns1.FullName.ShouldBe( "X" );
        ns1.Name.ShouldBe( "X" );
        ns1.Namespaces.ShouldHaveSingleItem().ShouldBe( ns2 );
        ns1.Types.ShouldBeEmpty();

        sut.Namespaces.ShouldHaveSingleItem().ShouldBe( ns1 );
        sut.Types.ShouldBeEmpty();
    }

    [Test]
    public void initialize_subnamespace()
    {
        INamespaceScope global = CreateWorkspace().Global;
        INamespaceScope sut = global.FindOrCreateNamespace( "X.Y" );
        INamespaceScope nested = sut.FindOrCreateNamespace( "Z" );

        sut.Namespaces.ShouldBe( new[] { nested } );
        nested.Parent.ShouldBeSameAs( sut );
        nested.FullName.ShouldBe( "X.Y.Z" );
        nested.Name.ShouldBe( "Z" );
        global.Namespaces.Select( ns => ns.FullName ).Single().ShouldBe( "X" );
    }

    [Test]
    public void find_existing_namespace()
    {
        INamespaceScope sut = CreateWorkspace().Global;
        INamespaceScope namespace1 = sut.FindOrCreateNamespace( "X.Y" );
        INamespaceScope namespace2 = sut.FindOrCreateNamespace( "X.Y" );

        namespace1.ShouldBeSameAs( namespace2 );
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
        ExtractNamespaces( nAXa ).ShouldBeEmpty();
        ExtractNamespaces( nAXb ).ShouldBeEmpty();
        ExtractNamespaces( nAX ).ShouldBeEmpty();
        ExtractNamespaces( nA ).ShouldBeEmpty();
        ExtractNamespaces( global ).ShouldAllBe( x => x == "EveryWhere" );

        nAXb.EnsureUsing( "Local" );
        ExtractNamespaces( nAXa ).ShouldBeEmpty();
        ExtractNamespaces( nAXb ).ShouldAllBe( x => x == "Local" );
        ExtractNamespaces( nAX ).ShouldBeEmpty();
        ExtractNamespaces( nA ).ShouldBeEmpty();
        ExtractNamespaces( global ).ShouldAllBe( x => x == "EveryWhere" );

        nAXa.EnsureUsing( "Local" );
        ExtractNamespaces( nAXa ).ShouldAllBe( x => x == "Local" );
        ExtractNamespaces( nAXb ).ShouldAllBe( x => x == "Local" );
        ExtractNamespaces( nAX ).ShouldBeEmpty();
        ExtractNamespaces( nA ).ShouldBeEmpty();
        ExtractNamespaces( global ).ShouldAllBe( x => x == "EveryWhere" );
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

        ExtractNamespaces( global ).ShouldAllBe( x => x == "INT" );
        ExtractNamespaces( nAXa ).ShouldBeEmpty();

        var source = workspace.GetGlobalSource();
        Normalize( source ).ShouldBe( Normalize(
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

        ExtractNamespaces( global ).ShouldAllBe( x => x == "INT" || x == "CNode" );
        ExtractNamespaces( nAXa ).ShouldAllBe( x => x == "CNode" );

        global.FindOrCreateNamespace( "ToShowTheTopLevelUsingsCopy" );
        source = workspace.GetGlobalSource();
        Normalize( source ).ShouldBe( Normalize(
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
        var d = (Dictionary<string, KeyValuePair<string, string>>?)n.GetType().GetField( "_usings", BindingFlags.NonPublic | BindingFlags.Instance )!.GetValue( n );
        Throw.DebugAssert( d != null );
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
