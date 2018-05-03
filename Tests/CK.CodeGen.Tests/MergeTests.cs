using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.CodeGen.Abstractions;

namespace CK.CodeGen.Tests
{
    [TestFixture]
    public class MergeTests
    {
        [Test]
        public void merging_code_space_with_one_type()
        {
            var c1 = CodeWorkspace.Create();
            var c2 = CodeWorkspace.Create();

            c1.EnsureAssemblyReference( typeof( object ), typeof( MergeTests ) );
            c2.EnsureAssemblyReference( typeof( Enumerable ), typeof( TestFixtureAttribute ), typeof( MergeTests ) );

            c1.Global.EnsureUsing( "A" );
            c2.Global.EnsureUsing( "B" );

            var ns1 = c1.Global.FindOrCreateNamespace( "Sub" );
            var t1 = ns1.CreateType( h => h.Append( "class C { //t1-Code" ).NewLine() );
            t1.Append( "//t1-MoreCode" );

            var ns2 = c2.Global.FindOrCreateNamespace( "Sub" );
            var t2 = ns2.CreateType( "class C { //t2-Code" + Environment.NewLine );
            t2.Append( "//t2-MoreCode" );

            c1.MergeWith( c2 );

            c1.AssemblyReferences.Should().BeEquivalentTo( typeof( object ).Assembly, typeof( Enumerable ).Assembly, typeof( TestFixtureAttribute ).Assembly, typeof( MergeTests ).Assembly );
            string code = c1.GetGlobalSource();
            code.Should().Be(
                "using A;" + Environment.NewLine +
                "using B;" + Environment.NewLine +
                "namespace Sub" + Environment.NewLine +
                "{" + Environment.NewLine +
                "class C { //t1-Code" + Environment.NewLine +
                "//t1-MoreCode" + "//t2-Code" + Environment.NewLine +
                "//t2-MoreCode" + Environment.NewLine +
                "}" + Environment.NewLine +
                "}" );
        }
    }
}
