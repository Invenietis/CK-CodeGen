using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.CodeGen;
using CK.Text;

namespace CK.CodeGen.Tests
{
    [TestFixture]
    public class CodePartTests
    {
        [Test]
        public void writing_before_the_using_statements()
        {
            {
                var g = CodeWorkspace.Create().Global;
                g.EnsureUsing( "A.Name.Space" );
                g.ToString().Should().StartWith( "using", "All the code is INSIDE the namespace and AFTER the usings." );
            }
            {
                var g = CodeWorkspace.Create().Global;
                g.Append( "AFTER" );
                g.BeforeNamespace.Append( "BEFORE" );
                g.EnsureUsing( "A.Name.Space" );
                g.ToString().Should().StartWith( "BEFORE" );
            }
        }

        [Test]
        public void BeforeNamespace_is_a_Part_that_can_have_Parts()
        {
            var g = CodeWorkspace.Create().Global;
            g.Append( "AFTER" );
            g.BeforeNamespace.Append( "BEFORE" );
            g.EnsureUsing( "A.Name.Space" );
            g.ToString().Should().StartWith( "BEFORE" );

            g.BeforeNamespace.CreatePart().Append( "POST" );
            g.BeforeNamespace.CreatePart( top: true ).Append( "ANTE" );

            g.ToString().Should().StartWith( "ANTE" + Environment.NewLine + "BEFORE" + Environment.NewLine + "POST" );

        }

        [Test]
        public void Function_can_use_lambda_definition()
        {
            var t = CodeWorkspace.Create().Global.CreateType( "class C" );
            var block = t.CreateFunction( "int B()" );
            block.Append( " " ).Append( " return 3;" );
            block.ToString().Replace( "\r", "" ).Replace( "\n", "" ).Should().Be( "int B(){  return 3;}" );

            var lambda = t.CreateFunction( "int L()" );
            lambda.Append( " " ).Append( " => 3;" );
            lambda.ToString().Replace( "\r", "" ).Replace( "\n", "" ).Should().Be( "int L()  => 3;" );
        }

        [Test]
        public void playing_with_parts()
        {
            INamespaceScope g = CodeWorkspace.Create().Global;
            INamespaceScopePart gSub = g.CreatePart();
            INamespaceScopePart gSub2 = gSub.CreatePart();
            ITypeScope gSub2Type1 = gSub2.CreateType( "class GSub2Type1" );
            ITypeScope gSub2Type2 = gSub2.CreateType( "class GSub2Type2" );
            ITypeScopePart gSub2Type1Part1 = gSub2Type1.CreatePart();
            ITypeScopePart gSub2Type1Part2 = gSub2Type1.CreatePart();
            IFunctionScope gSub2Type1Part2F1 = gSub2Type1Part2.CreateFunction( "void Action()" );
            IFunctionScopePart gSub2Type1Part2F1Part1 = gSub2Type1Part2F1.CreatePart();
            g.Append( "g:" );
            gSub.Append( "gSub:" );
            gSub2.Append( "gSub2:" );
            gSub2Type1.Append( "gSub2Type1:" );
            gSub2Type2.Append( "gSub2Type2:" );
            gSub2Type1Part1.Append( "gSub2Type1Part1:" );
            gSub2Type1Part2.Append( "gSub2Type1Part2:" );
            gSub2Type1Part2F1.Append( "gSub2Type1Part2F1:" );
            gSub2Type1Part2F1Part1.Append( "gSub2Type1Part2F1Part1:" );

            var s = g.ToString().Trim();
            s.Should().Be( @"
gSub2:
gSub:
g:
class GSub2Type1
{
gSub2Type1Part1:
gSub2Type1Part2:
gSub2Type1:
void Action()
{
gSub2Type1Part2F1Part1:
gSub2Type1Part2F1:
}
}
class GSub2Type2
{
gSub2Type2:
}".Trim().NormalizeEOL() );
        }

    }
}
