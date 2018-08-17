using System;
using FluentAssertions;
using NUnit.Framework;
using CK.CodeGen.Abstractions;

namespace CK.CodeGen.Tests
{
    public abstract class CommonTypeDefinerScopeTests
    {
        [TestCase( "  public class ClassName", "ClassName" )]
        [TestCase( "public class ClassName : Truc . Machin<T>", "ClassName" )]
        [TestCase( "public struct structName", "structName" )]
        [TestCase( "  public enum EnumName", "EnumName" )]
        [TestCase( "internal sealed class ClassName<T>", "ClassName<T>" )]
        [TestCase( "  public abstract class ClassName<T1, T2>", "ClassName<T1,T2>" )]
        [TestCase( "public interface IInterfaceName<T1, out T2, in T3, T4>", "IInterfaceName<T1,out T2,in T3,T4>" )]
        [TestCase( "public interface IInterfaceName<T1, out T2, in T3, T4> : ITest<T1>", "IInterfaceName<T1,out T2,in T3,T4>" )]
        [TestCase( "public interface IInterfaceName<T1,out T2,in T3,T4> { //...", "IInterfaceName<T1,out T2,in T3,T4>" )]
        [TestCase( "public interface interfacewhere <T1, out T2, in T3, T4> where T1 : struct { //...",
                            "interfacewhere<T1,out T2,in T3,T4>" )]
        [TestCase( "interface I<T1,out T2> : Z, B, A {", "I<T1,out T2>" )]
        public void created_type_has_normalized_Name( string decl, string typeName )
        {
            ITypeDefinerScope scope = CreateTypeDefinerScope();
            ITypeScope type = scope.CreateType( h => h.Append( decl ) );

            type.Name.Should().Be( typeName );
            type.FullName.Should().Be( $"{scope.FullName}.{typeName}" );

            scope.FindType( typeName ).Should().BeSameAs( type );

            scope.Invoking( sut => sut.CreateType( decl ) ).Should().Throw<ArgumentException>();
        }

        [TestCase( "interface I<T1,out T2> : Z, B, A {", "interface I<T1,out T2> : Z, A, B" )]
        [TestCase( " private enum   I< T1 ,out T2>:Z,B,A where Z:A where Y:A where X:Z,B,A {", "enum I<T1,out T2> : Z, A, B where X : Z, A, B where Y : A where Z : A" )]
        [TestCase( " private public struct I:Z,B,A where Z:A,new() where Y:A where X:Z,new(),B,A {", "public struct I : Z, A, B where X : Z, A, B, new() where Y : A where Z : A, new()" )]
        [TestCase( "[ Att ( 1 , @\" a \"\"str\"\" \" ) ] class A", "[Att(1,@\" a \"\"str\"\" \")]class A" )]
        [TestCase( "[ Z . K ( ) , Y , XAttribute ( \" a \\\"str\\\" \" ) ] class A", "[X(\" a \\\"str\\\" \"), Y, Z.K]class A" )]
        [TestCase( "[Z.K(1)][Y()][X(2)] class A", "[X(2), Y, Z.K(1)]class A" )]
        [TestCase( "[return:Z.K(' ')][Y()][return:X('\\'')] class A", "[return: X('\\''), Z.K(' ')][Y]class A" )]
        public void created_type_has_normalized_ToString_signature( string decl, string typeHeader )
        {
            ITypeDefinerScope scope = CreateTypeDefinerScope();
            ITypeScope type = scope.CreateType( decl );

            type.TypeHeader.Should().Be( typeHeader );
        }

        [TestCase( "public interface I<in T1, out T2> where T1 : struct { //...", "I<TKey,TValue>" )]
        [TestCase( "public interface I<in T1, out T2> where T1 : struct { //...", "I<,>" )]
        [TestCase( "class C<T<K,V>> where T1 : struct { //...", "C<>" )]
        [TestCase( "[A,B]class C", "[Other(\"str\")]C" )]
        public void creating_type_and_finding_them_back( string decl, string finder )
        {
            ITypeDefinerScope scope = CreateTypeDefinerScope();
            ITypeScope type = scope.CreateType( decl );

            scope.FindType( finder ).Should().BeSameAs( type );

            scope.Invoking( sut => sut.CreateType( decl ) ).Should().Throw<ArgumentException>();
        }

        [TestCase( "public sealed MissingKind" )]
        [TestCase( "public sealed class { // missing type name" )]
        [TestCase( "public sealed class : BaseClass // missing type name" )]
        [TestCase( "public sealed class  " )]
        [TestCase( "public sealed class" )]
        [TestCase( "public sealed class where T : struct" )]
        [TestCase( "public sealed class<T> where T : struct" )]
        [TestCase( "[] class C" )]
        [TestCase( "[-] class C" )]
        [TestCase( "[2] class C" )]
        public void create_type_with_invalid_header( string header )
        {
            ITypeDefinerScope scope = CreateTypeDefinerScope();
            scope.Invoking( sut => sut.CreateType( h => h.Append( header ) ) )
                     .Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void list_created_types()
        {
            ITypeDefinerScope scope = CreateTypeDefinerScope();
            ITypeScope t1 = scope.CreateType( s => s.Append( "public class C1" ) );
            ITypeScope t2 = scope.CreateType( s => s.Append( "public class C2" ) );

            scope.Types.Should().BeEquivalentTo( t1, t2 );
        }

        [TestCase( "public class C", "[A,B]internal class C" )]
        [TestCase( "public class C<T>", "public class C<TKey> : Base.X where TKey : K" )]
        [TestCase( "public class C", "readonly ref struct C" )]
        [TestCase( "public class C", "class C {" )]
        [TestCase( "public readonly ref struct C", "enum C {" )]
        [TestCase( "public class C", "enum C : byte" )]
        [TestCase( "class C < T1 , T2 >", "public class C<T1,T2>" )]
        [TestCase( "interface C<in T1, out T2>", "interface C<T1,T2>" )]
        public void creating_existing_type_again_clashes( string original, string clash )
        {
            ITypeDefinerScope scope = CreateTypeDefinerScope();
            scope.CreateType( original );
            scope.Invoking( sut => sut.CreateType( clash ) ).Should().Throw<ArgumentException>();
        }

        protected abstract ITypeDefinerScope CreateTypeDefinerScope();
    }
}
