using System;
using CK.Core;
using NUnit.Framework;
using Shouldly;

namespace CK.CodeGen.Tests;

[TestFixture]
public class TypeScopeTests : CommonTypeDefinerScopeTests
{
    [TestCase( "void M(int i)", "void" )]
    [TestCase( "L < T1, T2 > M<T>( int i, List<T> a )", "L<T1,T2>" )]
    [TestCase( "List<T,Dictionary<A,B>> M<L<T>,H<K>>( N<H> i, List<T> a )", "List<T,Dictionary<A,B>>" )]
    [TestCase( "ACONSTRuctor( N<H> i, List<T> a )", null )]
    [TestCase( "System.Int32 MMM()", "System.Int32" )]
    [TestCase( "public override Microsoft.Data.SqlClient.SqlCommand Do( ref System.Nullable<int> i )", "Microsoft.Data.SqlClient.SqlCommand" )]
    public void CreateFunction_extracts_return_type( string header, string returnType )
    {
        var t = CreateTypeScope();
        var f = t.CreateFunction( header );
        f.ShouldNotBeNull();
        f.IsConstructor.ShouldBe( returnType == null );
        if( !f.IsConstructor ) f.Definition.ReturnType!.ToString().ShouldBe( returnType );
    }


    [TestCase( "M(int i)", "M(int i)" )]
    [TestCase( "M(int i)", " M ( int i ) where K : class {" )]
    [TestCase( "M(int i = 9)", " M ( int i=10)" )]
    [TestCase( "M<T>(int i = 9)", " M <T> ( int i)" )]
    public void CreateFunction_clashes_on_same_name( string original, string clash )
    {
        var t = CreateTypeScope();
        var f = t.CreateFunction( original );
        f.ShouldNotBeNull();
        Util.Invokable( () => t.CreateFunction( clash ) ).ShouldThrow<ArgumentException>();
    }

    [TestCase( "M(int i)" )]
    [TestCase( "int M<T>(int i = 9, T?[]? p)" )]
    public void CreateFunction_with_FunctionDefinition( string original )
    {
        var t = CreateTypeScope();
        FunctionDefinition.TryParse( original, out var def ).ShouldBeTrue();
        var f = t.CreateFunction( def );
        f.ShouldNotBeNull();
        Util.Invokable( () => t.CreateFunction( def ) ).ShouldThrow<ArgumentException>();
    }

    [Test]
    public void using_FindOrCreateFunction_on_ctor_forbids_this_or_base_clause()
    {
        var t = CreateTypeScope();
        Util.Invokable(() => t.FindOrCreateFunction("C() : this()")).ShouldThrow<ArgumentException>();
        Util.Invokable(() => t.FindOrCreateFunction("C( int a ) : base( a )")).ShouldThrow<ArgumentException>();
    }

    [Test]
    public void using_FindOrCreateFunction_on_functions_forbids_any_body_start()
    {
        var t = CreateTypeScope();
        Util.Invokable(() => t.FindOrCreateFunction("int M() => 3;")).ShouldThrow<ArgumentException>();
        Util.Invokable(() => t.FindOrCreateFunction("int M( int a ) { // some code")).ShouldThrow<ArgumentException>();
    }

    [Test]
    public void using_CreateFunction_on_functions_handles_body_start()
    {
        var t = CreateTypeScope();
        var f1 = t.CreateFunction( "int M() => 3;" );
        f1.ToString().ShouldContain( "=> 3;" );

        var f2 = t.CreateFunction( "int M( int a ) { int a; (on a new line)." );
        f2.ToString().ShouldContain( "int a;" );
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
        c.IsConstructor.ShouldBeTrue();
        string result = text + Environment.NewLine + "{" + Environment.NewLine + "}";
        c.ToString().ShouldBe( result );
    }

    protected override ITypeDefinerScope CreateTypeDefinerScope() => CreateTypeScope();

    ITypeScope CreateTypeScope()
    {
        INamespaceScope global = CodeWorkspace.Create().Global;
        INamespaceScope ns = global.FindOrCreateNamespace( "A.Simple.Namespace" );
        return ns.CreateType( s => s.Append( "public class ClassName" ) );
    }
}
