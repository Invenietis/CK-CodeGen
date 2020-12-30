using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.CodeGen;
using FluentAssertions;
using System.Reflection;

namespace CK.CodeGen.Tests
{
    [TestFixture]
    public class CodeWriterTests
    {
        [Test]
        public void appending_an_unknown_typed_object_is_an_error()
        {
            var w = new StringCodeWriter();
            w.Invoking( x => x.Append( this ) ).Should().Throw<ArgumentException>();
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
        [TestCase( typeof( Above<>.Below<> ), "CK.CodeGen.Tests.CodeWriterTests.Above<>.Below<>" )]
        [TestCase( typeof( Above<long>.Below<int> ), "CK.CodeGen.Tests.CodeWriterTests.Above<long>.Below<int>" )]
        [TestCase( typeof( CodeWriterTests ), "CK.CodeGen.Tests.CodeWriterTests" )]
        [TestCase( typeof( List<string> ), "System.Collections.Generic.List<string>" )]
        [TestCase( typeof( List<Dictionary<int, string>> ), "System.Collections.Generic.List<System.Collections.Generic.Dictionary<int,string>>" )]
        [TestCase( typeof( Nullable<Guid> ), "System.Guid?" )]
        [TestCase( typeof( Guid? ), "System.Guid?" )]
        [TestCase( typeof( Another ), "CK.CodeGen.Tests.CodeWriterTests.Another" )]
        [TestCase( typeof( G<> ), "CK.CodeGen.Tests.CodeWriterTests.G<>" )]
        [TestCase( typeof( G<string> ), "CK.CodeGen.Tests.CodeWriterTests.G<string>" )]
        [TestCase( typeof( G<Another> ), "CK.CodeGen.Tests.CodeWriterTests.G<CK.CodeGen.Tests.CodeWriterTests.Another>" )]
        [TestCase( typeof( H<,> ), "CK.CodeGen.Tests.CodeWriterTests.H<,>" )]
        [TestCase( typeof( H<string, Another> ), "CK.CodeGen.Tests.CodeWriterTests.H<string,CK.CodeGen.Tests.CodeWriterTests.Another>" )]
        [TestCase( typeof( Another.I<> ), "CK.CodeGen.Tests.CodeWriterTests.Another.I<>" )]
        [TestCase( typeof( Another.I<int> ), "CK.CodeGen.Tests.CodeWriterTests.Another.I<int>" )]
        [TestCase( typeof( G<>.Nested ), "CK.CodeGen.Tests.CodeWriterTests.G<>.Nested" )]
        [TestCase( typeof( G<string>.Nested ), "CK.CodeGen.Tests.CodeWriterTests.G<string>.Nested" )]
        [TestCase( typeof( A<>.C<> ), "CK.CodeGen.Tests.CodeWriterTests.A<>.C<>" )]
        [TestCase( typeof( A<int>.C<string> ), "CK.CodeGen.Tests.CodeWriterTests.A<int>.C<string>" )]
        public void ToCSharpName_tests_without_generic_parameter_names( Type type, string expected )
        {
            Assert.AreEqual( expected, type.ToCSharpName( false ) );
        }

        [TestCase( typeof( Dictionary<,>.KeyCollection ), "System.Collections.Generic.Dictionary<TKey,TValue>.KeyCollection" )]
        [TestCase( typeof( Dictionary<int, string>.KeyCollection ), "System.Collections.Generic.Dictionary<int,string>.KeyCollection" )]
        [TestCase( typeof( Above<>.Below<> ), "CK.CodeGen.Tests.CodeWriterTests.Above<T1>.Below<T1>" )]
        [TestCase( typeof( Nullable<> ), "System.Nullable<T>" )]
        [TestCase( typeof( G<> ), "CK.CodeGen.Tests.CodeWriterTests.G<T>" )]
        [TestCase( typeof( H<,> ), "CK.CodeGen.Tests.CodeWriterTests.H<T1,T2>" )]
        [TestCase( typeof( Another.I<> ), "CK.CodeGen.Tests.CodeWriterTests.Another.I<T3>" )]
        [TestCase( typeof( G<>.Nested ), "CK.CodeGen.Tests.CodeWriterTests.G<T>.Nested" )]
        [TestCase( typeof( A<>.C<> ), "CK.CodeGen.Tests.CodeWriterTests.A<TB>.C<TD>" )]
        [TestCase( typeof( int[] ), "int[]" )]
        [TestCase( typeof( Byte[,,,] ), "byte[,,,]" )]
        [TestCase( typeof( int? ), "int?" )]
        [TestCase( typeof( int?[] ), "int?[]" )]
        public void ToCSharpName_tests( Type type, string expected )
        {
            var writer = new StringCodeWriter( new StringBuilder() );
            writer.AppendCSharpName( type );
            Assert.AreEqual( expected, writer.ToString() );
        }

        // Cannot use TestCase parameters: parentheses trigger an error that prevents the test to run.
        //
        // An exception occurred while invoking executor 'executor://nunit3testexecutor/': Incorrect format for TestCaseFilter Error:
        // Missing '('. Specify the correct format and try again. Note that the incorrect format can lead to no test getting executed.
        //
        [Test]
        public void ToCSharpName_tests_value_types()
        {
            {
                var writer = new StringCodeWriter( new StringBuilder() );
                writer.AppendCSharpName( typeof( (int, string) ) );
                writer.ToString().Should().Be( "(int,string)" );
            }
            {
                var writer = new StringCodeWriter( new StringBuilder() );
                writer.AppendCSharpName( typeof( (int, (string,float)) ) );
                writer.ToString().Should().Be( "(int,(string,float))" );
            }
        }

        public class G<T>
        {
            public class Nested
            {
                public A<int?>.C<string>? Prop { get; set; }
            }
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
