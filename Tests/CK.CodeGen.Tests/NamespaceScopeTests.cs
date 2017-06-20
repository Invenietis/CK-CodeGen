using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.CodeGen.Abstractions;
using NUnit.Framework;

namespace CK.CodeGen.Tests
{
    [TestFixture]
    public class NamespaceScopeTests : Abstractions.Tests.NamespaceScopeTests
    {
        protected override INamespaceScope CreateNamespaceScope( string ns ) => CodeScope.CreateNamespace( ns );
    }
}
