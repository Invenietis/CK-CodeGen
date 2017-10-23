using System;
using CK.CodeGen.Abstractions;
using NUnit.Framework;
using FluentAssertions;

namespace CK.CodeGen.Tests
{
    [TestFixture]
    public class TypeScopeTests : Abstractions.Tests.TypeDefinerScopeTests
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

        [TestCase( "void M(int i)", "M(inti)" )]
        [TestCase( "L < T1, T2 > M<T>( int i, List< T > a )", "M<T>(inti,List<T>a)" )]
        [TestCase( "List<T,Dictionary<A,B>> M<L<T>,H<K>>( N<H> i, List<T> a )", "M<L<T>,H<K>>(N<H>i,List<T>a)" )]
        [TestCase( "ACONSTRuctor( N<H> i, List<T> a )", "ACONSTRuctor(N<H>i,List<T>a)" )]
        [TestCase("System.Int32 MMM()", "MMM()")]
        [TestCase( "public override System.Data.SqlClient.SqlCommand Do( ref System.Nullable<int> i )", "Do(refSystem.Nullable<int>i)" )]
        public void CreateFunction_has_a_currently_very_stupid_name_handling( string header, string name )
        {
            var t = CreateTypeScope();
            var f = t.CreateFunction( header );
            f.Should().NotBeNull();
            f.Name.Should().Be( name );
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
