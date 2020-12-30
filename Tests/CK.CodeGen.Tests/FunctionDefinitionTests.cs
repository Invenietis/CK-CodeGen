using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen.Tests
{
    [TestFixture]
    public class FunctionDefinitionTests
    {
        [Test]
        public void FunctionDefinition_are_identified_by_their_keys()
        {
            FunctionDefinition.TryParse( "void Print( int i )", out var m1 ).Should().BeTrue();
            FunctionDefinition.TryParse( "Other Print(int notI)", out var m2 ).Should().BeTrue();
            m1.Key.Should().Be( m2.Key );
            FunctionDefinition.TryParse( "void print( int notI )", out var m3 ).Should().BeTrue();
            m1.Key.Should().NotBe( m3.Key );
            FunctionDefinition.TryParse( "void Print( uint i )", out var m4 ).Should().BeTrue();
            m1.Key.Should().NotBe( m4.Key );
        }

        [Test]
        public void FunctionDefinition_Keys_normalize_the_generic_names()
        {
            FunctionDefinition.TryParse( "void M<T,U>( T i, List<U> j )", out var m1 ).Should().BeTrue();
            FunctionDefinition.TryParse( "int M<I,J>(I x,List<J> y)", out var m2 ).Should().BeTrue();
            m1.Key.Should().Be( m2.Key );
        }

        [TestCase( "M(Nullable<int> i)", "M(int?)" )]
        [TestCase( "void M(int i)", "M(int)" )]
        [TestCase( "L < T1, T2 > M < T > ( int i , List < T >a )", "M`1(int,List<§0>)" )]
        [TestCase( "ACONSTRuctor( N<H> i, List<T> a )", "ACONSTRuctor(N<H>,List<T>)" )]
        [TestCase( "System.Int32 MMM()", "MMM()" )]
        [TestCase( "public override System.Data.SqlClient.SqlCommand Do( out System.Nullable<int> i )", "Do(&int?)" )]
        [TestCase( "R M < X , K > ( [ A ( 1 ) ] [ A ] ref System . Nullable < K > i = \"\",params K[,,] p = new(){ nimp, 0x9876UL })", "M`2(&§1?,§1[,,])" )]
        [TestCase( "void Write( ref Reader r, ref (int, string) v )", "Write(&Reader,&(int,string))" )]
        public void CreateFunction_normalizes_its_key( string header, string key )
        {
            FunctionDefinition.TryParse( header, out var f );
            f.Key.Should().Be( key );
        }

        [TestCase( "M(Nullable<int> i)", "M(int?)" )]
        [TestCase( "void M(int? i)", "M(int?)" )]
        [TestCase( "void M(int? i, Truc? t)", "M(int?,Truc?)" )]
        [TestCase( "void M<T>( ISet<ISet<T?>?>? t)", "M`1(ISet<ISet<§0?>?>?)" )]
        public void Function_Keys_are_nullable_sensitive( string header, string key )
        {
            FunctionDefinition.TryParse( header, out var f );
            f.Key.Should().Be( key );
        }
    }
}
