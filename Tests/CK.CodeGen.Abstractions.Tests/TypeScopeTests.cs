using System;
using FluentAssertions;
using NUnit.Framework;

namespace CK.CodeGen.Abstractions.Tests
{
    public abstract class TypeScopeTests
    {
        [TestCase( "public class ClassName", "ClassName" )]
        [TestCase( "public struct StructName", "StructName" )]
        [TestCase( "public enum EnumName", "EnumName" )]
        [TestCase( "internal sealed class ClassName<T>", "ClassName<T>" )]
        [TestCase( "public abstract class ClassName<T1, T2>", "ClassName<T1,T2>" )]
        [TestCase( "public interface IInterfaceName<T1, out T2, in T3, T4>", "IInterfaceName<T1,out T2,in T3,T4>" )]
        [TestCase( "public interface IInterfaceName<T1, out T2, in T3, T4> : ITest<T1>", "IInterfaceName<T1,out T2,in T3,T4>" )]
        [TestCase( "public interface IInterfaceName<T1, out T2, in T3, T4> { //...", "IInterfaceName<T1,out T2,in T3,T4>" )]
        public void handle_type_name( string decl, string typeName )
        {
            INamespaceScope global = CreateGlobalNamespace();
            INamespaceScope ns = global.FindOrCreateNamespace( "X.Y.Z" );
            ITypeScope sut = ns.CreateType( h => h.Builder.Append( decl ) );

            sut.FullName.Should().Be( "X.Y.Z." + typeName );
            sut.Name.Should().Be( typeName );
        }

        [Test]
        public void missing_type_kind()
        {
            INamespaceScope global = CreateGlobalNamespace();
            INamespaceScope ns = global.FindOrCreateNamespace( "X.Y.Z" );
            ns.Invoking( s => s.CreateType( h => h.Builder.Append( "public sealed ClassName" ) ) ).ShouldThrow<InvalidOperationException>();
        }

        protected abstract INamespaceScope CreateGlobalNamespace();
    }
}
