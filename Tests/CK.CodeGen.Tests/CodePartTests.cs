using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.CodeGen.Abstractions;
using CK.Text;

namespace CK.CodeGen.Tests
{
    [TestFixture]
    public class CodePartTests
    {
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
gSub:
gSub2:
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
}
".Trim().NormalizeEOL().Trim() );
        }

    }
}
