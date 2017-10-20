using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen.Abstractions.Tests
{
    public abstract class CodeWriterTests
    {
        [Test]
        public void appending_an_unknown_typed_object_is_an_error()
        {
            var w = new SimpleCodeWriter();
            w.Invoking( x => x.Append( this ) ).ShouldThrow<ArgumentException>();
        }

#pragma warning disable CS0693
        public class Above<T1>
        {
            // This triggers a CS0693 warning:
            // Type parameter 'T1' has the same name as the type parameter from outer type 'ToCSharpNameTests.Above<T1>'
            public class Below<T1>
            {
            }
        }
#pragma warning restore CS0693

        [TestCase( typeof( Dictionary<,>.KeyCollection ), "System.Collections.Generic.Dictionary<,>.KeyCollection" )]
        [TestCase( typeof( Dictionary<int, string>.KeyCollection ), "System.Collections.Generic.Dictionary<int,string>.KeyCollection" )]
        [TestCase( typeof( Above<>.Below<> ), "CK.CodeGen.Abstractions.Tests.CodeWriterTests.Above<>.Below<>" )]
        [TestCase( typeof( Above<long>.Below<int> ), "CK.CodeGen.Abstractions.Tests.CodeWriterTests.Above<long>.Below<int>" )]
        [TestCase( typeof( CodeWriterTests ), "CK.CodeGen.Abstractions.Tests.CodeWriterTests" )]
        [TestCase( typeof( List<string> ), "System.Collections.Generic.List<string>" )]
        [TestCase( typeof( List<Dictionary<int, string>> ), "System.Collections.Generic.List<System.Collections.Generic.Dictionary<int,string>>" )]
        [TestCase( typeof( Nullable<Guid> ), "System.Nullable<System.Guid>" )]
        [TestCase( typeof( Guid? ), "System.Nullable<System.Guid>" )]
        [TestCase( typeof( Another ), "CK.CodeGen.Abstractions.Tests.CodeWriterTests.Another" )]
        [TestCase( typeof( G<> ), "CK.CodeGen.Abstractions.Tests.CodeWriterTests.G<>" )]
        [TestCase( typeof( G<string> ), "CK.CodeGen.Abstractions.Tests.CodeWriterTests.G<string>" )]
        [TestCase( typeof( G<Another> ), "CK.CodeGen.Abstractions.Tests.CodeWriterTests.G<CK.CodeGen.Abstractions.Tests.CodeWriterTests.Another>" )]
        [TestCase( typeof( H<,> ), "CK.CodeGen.Abstractions.Tests.CodeWriterTests.H<,>" )]
        [TestCase( typeof( H<string, Another> ), "CK.CodeGen.Abstractions.Tests.CodeWriterTests.H<string,CK.CodeGen.Abstractions.Tests.CodeWriterTests.Another>" )]
        [TestCase( typeof( Another.I<> ), "CK.CodeGen.Abstractions.Tests.CodeWriterTests.Another.I<>" )]
        [TestCase( typeof( Another.I<int> ), "CK.CodeGen.Abstractions.Tests.CodeWriterTests.Another.I<int>" )]
        [TestCase( typeof( G<>.Nested ), "CK.CodeGen.Abstractions.Tests.CodeWriterTests.G<>.Nested" )]
        [TestCase( typeof( G<string>.Nested ), "CK.CodeGen.Abstractions.Tests.CodeWriterTests.G<string>.Nested" )]
        [TestCase( typeof( A<>.C<> ), "CK.CodeGen.Abstractions.Tests.CodeWriterTests.A<>.C<>" )]
        [TestCase( typeof( A<int>.C<string> ), "CK.CodeGen.Abstractions.Tests.CodeWriterTests.A<int>.C<string>" )]
        public void ToCSharpName_tests( Type type, string expected )
        {
            var writer = new SimpleCodeWriter();
            writer.AppendCSharpName( type, false );
            Assert.AreEqual( expected, writer.ToString() );
        }

        [TestCase( typeof( Dictionary<,>.KeyCollection ), "System.Collections.Generic.Dictionary<TKey,TValue>.KeyCollection" )]
        [TestCase( typeof( Dictionary<int, string>.KeyCollection ), "System.Collections.Generic.Dictionary<int,string>.KeyCollection" )]
        [TestCase( typeof( Above<>.Below<> ), "CK.CodeGen.Abstractions.Tests.CodeWriterTests.Above<T1>.Below<T1>" )]
        [TestCase( typeof( Nullable<> ), "System.Nullable<T>" )]
        [TestCase( typeof( G<> ), "CK.CodeGen.Abstractions.Tests.CodeWriterTests.G<T>" )]
        [TestCase( typeof( H<,> ), "CK.CodeGen.Abstractions.Tests.CodeWriterTests.H<T1,T2>" )]
        [TestCase( typeof( Another.I<> ), "CK.CodeGen.Abstractions.Tests.CodeWriterTests.Another.I<T3>" )]
        [TestCase( typeof( G<>.Nested ), "CK.CodeGen.Abstractions.Tests.CodeWriterTests.G<T>.Nested" )]
        [TestCase( typeof( A<>.C<> ), "CK.CodeGen.Abstractions.Tests.CodeWriterTests.A<TB>.C<TD>" )]
        public void ToCSharpName_tests_with_generic_parameter_names( Type type, string expected )
        {
            var writer = new SimpleCodeWriter();
            writer.AppendCSharpName( type );
            Assert.AreEqual( expected, writer.ToString() );
        }

        public class G<T>
        {
            public class Nested { }
        }

        public class A<TB>
        {
            public class C<TD> { }
        }

        public class H<T1, T2> { }

        public class Another
        {
            public class I<T3> { }
        }

    }
}
