using CK.CodeGen.Abstractions;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen.Tests
{
    [TestFixture]
    public class TypeDefinitionTests
    {
        [Test]
        public void TypeDefinition_can_be_modified()
        {
            INamespaceScope global = CodeWorkspace.Create().Global;
            var t = global.CreateType( "public class Derived : Base" );
            t.TypeDefinition.BaseTypes.Should().BeEquivalentTo( new ExtendedTypeName( "Base" ) );
            t.TypeDefinition.BaseTypes.Add( new ExtendedTypeName( "Name.Space.IOther" ) );
            global.ToString().Should().Contain( "Name.Space.IOther" );
        }


        [Test]
        public void TypeDefinition_can_start_the_body()
        {
            INamespaceScope global = CodeWorkspace.Create().Global;
            var t = global.CreateType( "public class Derived : Base { truc" );
            t.TypeDefinition.BaseTypes.Add( new ExtendedTypeName( "Name.Space.IOther" ) );
            var s = global.ToString();
            s = s.Replace( Environment.NewLine, "" ).Replace( " ", "" );
            s.Should().EndWith( "Name.Space.IOther{truc}" );
        }
    }
}
