using System;
using CK.CodeGen;
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
        [TestCase( "public override Microsoft.Data.SqlClient.SqlCommand Do( ref System.Nullable<int> i )", "Microsoft.Data.SqlClient.SqlCommand" )]
        public void CreateFunction_extracts_return_type( string header, string returnType )
        {
            var t = CreateTypeScope();
            var f = t.CreateFunction( header );
            f.Should().NotBeNull();
            f.IsConstructor.Should().Be( returnType == null );
            if( !f.IsConstructor ) f.Definition.ReturnType!.ToString().Should().Be( returnType );
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

        [TestCase( "M(int i)" )]
        [TestCase( "int M<T>(int i = 9, T?[]? p)")]
        public void CreateFunction_with_FunctionDefinition( string original )
        {
            var t = CreateTypeScope();
            FunctionDefinition.TryParse( original, out var def ).Should().BeTrue();
            var f = t.CreateFunction( def );
            f.Should().NotBeNull();
            t.Invoking( x => x.CreateFunction( def ) ).Should().Throw<ArgumentException>();
        }

        [Test]
        public void using_FindOrCreateFunction_on_ctor_forbids_this_or_base_clause()
        {
            var t = CreateTypeScope();
            t.Invoking( x => x.FindOrCreateFunction( "C() : this()" ) ).Should().Throw<ArgumentException>();
            t.Invoking( x => x.FindOrCreateFunction( "C( int a ) : base( a )" ) ).Should().Throw<ArgumentException>();
        }

        [Test]
        public void using_FindOrCreateFunction_on_functions_forbids_any_body_start()
        {
            var t = CreateTypeScope();
            t.Invoking( x => x.FindOrCreateFunction( "int M() => 3;" ) ).Should().Throw<ArgumentException>();
            t.Invoking( x => x.FindOrCreateFunction( "int M( int a ) { // some code" ) ).Should().Throw<ArgumentException>();
        }

        [Test]
        public void using_CreateFunction_on_functions_handles_body_start()
        {
            var t = CreateTypeScope();
            var f1 = t.CreateFunction( "int M() => 3;" );
            f1.ToString().Should().Contain( "=> 3;" );

            var f2 = t.CreateFunction( "int M( int a ) { int a; (on a new line)." );
            f2.ToString().Should().Contain( "int a;" );
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
