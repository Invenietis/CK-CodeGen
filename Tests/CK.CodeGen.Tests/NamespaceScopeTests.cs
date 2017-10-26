using System;
using CK.CodeGen.Abstractions;
using NUnit.Framework;
using System.Collections.Generic;
using System.Reflection;
using FluentAssertions;
using System.Text.RegularExpressions;

namespace CK.CodeGen.Tests
{
    [TestFixture]
    public class NamespaceScopeTests : Abstractions.Tests.NamespaceScopeTests
    {
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
                @"using INT = System.UInt8;
namespace A
{
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

            source = workspace.GetGlobalSource();
            Normalize( source ).Should().Be( Normalize(
                @"using INT = System.UInt8;
                  using CNode = SNode<string,bool>;
                  namespace A {
                    namespace X {
                      namespace a {
                        using CNode = SNode<STRING, BOOL>;
                        // The 2 CNode definition differ.
                      }
                      namespace b {
                      }
                    }
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

        protected override ICodeWorkspace CreateWorkspace() => CodeWorkspace.Create();
    }
}
