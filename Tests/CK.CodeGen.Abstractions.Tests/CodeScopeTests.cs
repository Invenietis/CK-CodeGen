using System;
using FluentAssertions;
using NUnit.Framework;

namespace CK.CodeGen.Abstractions.Tests
{
    public abstract class CodeScopeTests
    {
        [TestCase( "public class ClassName", "ClassName" )]
        [TestCase( "public struct StructName", "StructName" )]
        [TestCase( "public enum EnumName", "EnumName" )]
        [TestCase( "internal sealed class ClassName<T>", "ClassName<T>" )]
        [TestCase( "public abstract class ClassName<T1, T2>", "ClassName<T1,T2>" )]
        [TestCase( "public interface IInterfaceName<T1, out T2, in T3, T4>", "IInterfaceName<T1,out T2,in T3,T4>" )]
        [TestCase( "public interface IInterfaceName<T1, out T2, in T3, T4> : ITest<T1>", "IInterfaceName<T1,out T2,in T3,T4>" )]
        [TestCase( "public interface IInterfaceName<T1, out T2, in T3, T4> { //...", "IInterfaceName<T1,out T2,in T3,T4>" )]
        public void create_type( string decl, string typeName )
        {
            ICodeScope sut = CreateCodeScope();
            ITypeScope type = sut.CreateType( h => h.Builder.Append( decl ) );

            type.FullName.Should().Be( string.Format( "{0}.{1}", sut.FullName, typeName ) );
            type.Name.Should().Be( typeName );
        }

        [TestCase( "public sealed MissingKind" )]
        [TestCase( "public sealed class { // missing type name" )]
        [TestCase( "public sealed class : BaseClass // missing type name" )]
        [TestCase( "public sealed class  " )]
        [TestCase( "public sealed class" )]
        public void create_type_with_invalid_header( string header )
        {
            ICodeScope codeScope = CreateCodeScope();
            codeScope.Invoking( sut => sut.CreateType( h => h.Builder.Append( header ) ) )
                     .ShouldThrow<InvalidOperationException>();
        }

        protected abstract ICodeScope CreateCodeScope();
    }
}
