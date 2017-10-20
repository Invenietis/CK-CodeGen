using System;
using FluentAssertions;
using NUnit.Framework;

namespace CK.CodeGen.Abstractions.Tests
{
    public abstract class TypeScopeTests : CodeScopeTests
    {
        protected abstract ITypeScope CreateTypeScope();

        protected override ICodeScope CreateCodeScope() => CreateTypeScope();
    }
}
