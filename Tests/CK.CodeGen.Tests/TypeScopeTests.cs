using System;
using CK.CodeGen.Abstractions;
using NUnit.Framework;
using FluentAssertions;

namespace CK.CodeGen.Tests
{
    [TestFixture]
    public class TypeScopeTests : CommonTypeDefinerScopeTests
    {
        [TestCase( "void M(int i)", "void" )]
        [TestCase( "L < T1, T2 > M<T>( int i, List<T> a )", "L<T1,T2>" )]
        [TestCase( "List<T,Dictionary<A,B>> M<L<T>,H<K>>( N<H> i, List<T> a )", "List<T,Dictionary<A,B>>" )]
        [TestCase( "ACONSTRuctor( N<H> i, List<T> a )", null )]
        [TestCase( "System.Int32 MMM()", "System.Int32")]
        [TestCase( "public override System.Data.SqlClient.SqlCommand Do( ref System.Nullable<int> i )", "System.Data.SqlClient.SqlCommand" )]
        public void CreateFunction_extracts_return_type( string header, string returnType )
        {
            var t = CreateTypeScope();
            var f = t.CreateFunction( header );
            f.Should().NotBeNull();
            f.IsConstructor.Should().Be( returnType == null );
            f.ReturnType.Should().Be( returnType );
        }

        [TestCase( "void M(int i)", "M( int i )" )]
        [TestCase( "L < T1, T2 > M < T > ( int i , List < T >a )", "M<T>( int i, List<T> a )" )]
        [TestCase( "List<T,Dictionary<A,B>> M<L<T>,H<K>>( N < H > i , List<T> a )", "M<L<T>,H<K>>( N<H> i, List<T> a )" )]
        [TestCase( "ACONSTRuctor( N<H> i, List<T> a )", "ACONSTRuctor( N<H> i, List<T> a )" )]
        [TestCase( "System.Int32 MMM()", "MMM()" )]
        [TestCase( "public override System.Data.SqlClient.SqlCommand Do( ref System.Nullable<int> i )", "Do( ref System.Nullable<int> i )" )]
        [TestCase( "R M( [ A ( 1 ) ] [ A ] ref System . Nullable < int > i = \"\",params K[,,] p = new(){ nimp, 0x9876UL })", "M( ref System.Nullable<int> i, params K[,,] p )" )]
        public void CreateFunction_normalizes_the_function_name( string header, string name )
        {
            var t = CreateTypeScope();
            var f = t.CreateFunction( header );
            f.Should().NotBeNull();
            f.Name.Should().Be( name );
        }

        [TestCase( "M(int i)", "M(int i)" )]
        [TestCase( "M(int i)", " M ( int i ) where K : class {" )]
        [TestCase( "M(int i = 9)", " M ( int i=10)" )]
        [TestCase( "M<T>(int i = 9)", " M <T> ( int i)" )]
        public void CreateFunction_clashes_on_same_name( string original, string clash )
        {
            var t = CreateTypeScope();
            var f = t.CreateFunction( original );
            f.Should().NotBeNull();
            t.Invoking( x => x.CreateFunction( clash ) ).Should().Throw<ArgumentException>();
        }

        [TestCase( "public C()" )]
        [TestCase( "public C() : base( 3 )" )]
        [TestCase( "public C( X a, Y b ) : this( a*a , typeof(T) )" )]
        [TestCase( "public C() : base( new[]{ new O(), (null) }, Kilo )" )]
        [TestCase( @"public S1() : base( typeof( SFront1 ),
new[]{
new StObjServiceParameterInfo( typeof(ISBase), 0, @""next"", I0)
} )" )]
        public void handling_constructors( string text )
        {
            var t = CreateTypeScope();
            var c = t.CreateFunction( ctor =>
            {
                ctor.Append( text );
            } );
            c.IsConstructor.Should().BeTrue();
            string result = text + Environment.NewLine + "{" + Environment.NewLine + "}";
            c.ToString().Should().Be( result );
        }

        protected override ITypeDefinerScope CreateTypeDefinerScope() => CreateTypeScope();

        ITypeScope CreateTypeScope()
        {
            INamespaceScope global = CodeWorkspace.Create().Global;
            INamespaceScope ns = global.FindOrCreateNamespace( "A.Simple.Namespace" );
            return ns.CreateType( s => s.Append( "public class ClassName" ) );
        }
    }
}
