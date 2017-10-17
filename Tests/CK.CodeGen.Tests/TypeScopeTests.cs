using System;
using CK.CodeGen.Abstractions;
using NUnit.Framework;

namespace CK.CodeGen.Tests
{
    [TestFixture]
    public class TypeScopeTests : Abstractions.Tests.TypeScopeTests
    {
        protected override ITypeScope CreateTypeScope()
        {
            INamespaceScope global = CodeScope.CreateGlobalNamespace();
            INamespaceScope ns = global.FindOrCreateNamespace( "A.Simple.Namespace" );
            return ns.CreateType( s => s.Append( "public class ClassName" ) );
        }
    }
}
