using System;
using FluentAssertions;
using NUnit.Framework;

namespace CK.CodeGen.Abstractions.Tests
{
    public abstract class TypeDefinerScopeTests
    {
        [TestCase( "public class ClassName", "ClassName" )]
        [TestCase( "public struct structName", "structName" )]
        [TestCase( "public enum EnumName", "EnumName" )]
        [TestCase( "internal sealed class ClassName<T>", "ClassName<T>" )]
        [TestCase( "public abstract class ClassName<T1, T2>", "ClassName<T1,T2>" )]
        [TestCase( "public interface IInterfaceName<T1, out T2, in T3, T4>", "IInterfaceName<T1,T2,T3,T4>" )]
        [TestCase( "public interface IInterfaceName<T1, out T2, in T3, T4> : ITest<T1>", "IInterfaceName<T1,T2,T3,T4>" )]
        [TestCase( "public interface IInterfaceName<T1,out T2,in T3,T4> { //...", "IInterfaceName<T1,T2,T3,T4>" )]
        [TestCase( "public interface interfacewhere <T1, out T2, in T3, T4> where T1 : struct { //...", "interfacewhere<T1,T2,T3,T4>" )]
        public void create_type( string decl, string typeName )
        {
            ITypeDefinerScope scope = CreateTypeDefinerScope();
            ITypeScope type = scope.CreateType( h => h.Append( decl ) );

            type.Name.Should().Be( typeName );
            type.FullName.Should().Be( $"{scope.FullName}.{typeName}" );

            scope.FindType( typeName ).Should().BeSameAs( type );
        }

        [TestCase( "public sealed MissingKind" )]
        [TestCase( "public sealed class { // missing type name" )]
        [TestCase( "public sealed class : BaseClass // missing type name" )]
        [TestCase( "public sealed class  " )]
        [TestCase( "public sealed class" )]
        [TestCase( "public sealed class where T : struct" )]
        [TestCase( "public sealed class<T> where T : struct" )]
        public void create_type_with_invalid_header( string header )
        {
            ITypeDefinerScope scope = CreateTypeDefinerScope();
            scope.Invoking( sut => sut.CreateType( h => h.Append( header ) ) )
                     .ShouldThrow<InvalidOperationException>();
        }

        [Test]
        public void obtain_created_types()
        {
            ITypeDefinerScope scope = CreateTypeDefinerScope();
            ITypeScope t1 = scope.CreateType( s => s.Append( "public class C1" ) );
            ITypeScope t2 = scope.CreateType( s => s.Append( "public class C2" ) );

            scope.Types.Should().BeEquivalentTo( t1, t2 );
        }

        [Test]
        public void find_type()
        {
            ITypeDefinerScope scope = CreateTypeDefinerScope();
            ITypeScope t = scope.CreateType( s => s.Append( "public class C" ) );

            scope.FindType( "C" ).Should().BeSameAs( t );
        }

        [TestCase( "public class C", "internal class C" )]
        [TestCase( "public class C", "public class C : Base" )]
        [TestCase( "public class C", "struct C" )]
        [TestCase( "public class C", "class C {" )]
        [TestCase( "public class C", "enum C {" )]
        [TestCase( "public class C", "enum C : byte" )]
        [TestCase( "class C < T1 , T2 >", "public class C<T1,T2>" )]
        [TestCase( "interface C<in T1, out T2>", "interface C<T1,T2>" )]
        public void create_existing_type_again( string original, string duplicate )
        {
            ITypeDefinerScope scope = CreateTypeDefinerScope();
            scope.CreateType( s => s.Append( original ) );
            scope.Invoking( sut => sut.CreateType( s => s.Append( duplicate ) ) )
                     .ShouldThrow<ArgumentException>();
        }

        protected abstract ITypeDefinerScope CreateTypeDefinerScope();
    }
}