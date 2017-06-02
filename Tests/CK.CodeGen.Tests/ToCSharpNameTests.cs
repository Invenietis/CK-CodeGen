using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CK.CodeGen.Tests
{
    [TestFixture]
    public class ToCSharpNameTests
    {

        public class Above<T1>
        {
            // This triggers a warning: same generic parameter name.
            public class Below<T1>
            {
            }
        }

        [TestCase( typeof( Dictionary<,>.KeyCollection ), "System.Collections.Generic.Dictionary<,>.KeyCollection" )]
        [TestCase( typeof( Dictionary<int, string>.KeyCollection ), "System.Collections.Generic.Dictionary<int,string>.KeyCollection" )]
        [TestCase( typeof( Above<>.Below<> ), "CK.CodeGen.Tests.ToCSharpNameTests.Above<>.Below<>" )]
        [TestCase( typeof( Above<long>.Below<int> ), "CK.CodeGen.Tests.ToCSharpNameTests.Above<long>.Below<int>" )]
        [TestCase( typeof( ToCSharpNameTests ), "CK.CodeGen.Tests.ToCSharpNameTests" )]
        [TestCase( typeof( List<string> ), "System.Collections.Generic.List<string>" )]
        [TestCase( typeof( List<Dictionary<int, string>> ), "System.Collections.Generic.List<System.Collections.Generic.Dictionary<int,string>>" )]
        [TestCase( typeof( Nullable<Guid> ), "System.Nullable<System.Guid>" )]
        [TestCase( typeof( Guid? ), "System.Nullable<System.Guid>" )]
        [TestCase( typeof( CK.CodeGen.Tests.ToCSharpNameTests.Another ), "CK.CodeGen.Tests.ToCSharpNameTests.Another" )]
        [TestCase( typeof( G<> ), "CK.CodeGen.Tests.ToCSharpNameTests.G<>" )]
        [TestCase( typeof( G<string> ), "CK.CodeGen.Tests.ToCSharpNameTests.G<string>" )]
        [TestCase( typeof( G<Another> ), "CK.CodeGen.Tests.ToCSharpNameTests.G<CK.CodeGen.Tests.ToCSharpNameTests.Another>" )]
        [TestCase( typeof( H<,> ), "CK.CodeGen.Tests.ToCSharpNameTests.H<,>" )]
        [TestCase( typeof( H<string, Another> ), "CK.CodeGen.Tests.ToCSharpNameTests.H<string,CK.CodeGen.Tests.ToCSharpNameTests.Another>" )]
        [TestCase( typeof( Another.I<> ), "CK.CodeGen.Tests.ToCSharpNameTests.Another.I<>" )]
        [TestCase( typeof( Another.I<int> ), "CK.CodeGen.Tests.ToCSharpNameTests.Another.I<int>" )]
        [TestCase( typeof( G<>.Nested ), "CK.CodeGen.Tests.ToCSharpNameTests.G<>.Nested" )]
        [TestCase( typeof( G<string>.Nested ), "CK.CodeGen.Tests.ToCSharpNameTests.G<string>.Nested" )]
        [TestCase( typeof( A<>.C<> ), "CK.CodeGen.Tests.ToCSharpNameTests.A<>.C<>" )]
        [TestCase( typeof( A<int>.C<string> ), "CK.CodeGen.Tests.ToCSharpNameTests.A<int>.C<string>" )]
        public void ToCSharpName_tests( Type type, string expected )
        {
            string actual = type.ToCSharpName( false );
            Assert.AreEqual( expected, actual );
        }


        [TestCase( typeof( Dictionary<,>.KeyCollection ), "System.Collections.Generic.Dictionary<TKey,TValue>.KeyCollection" )]
        [TestCase( typeof( Above<>.Below<> ), "CK.CodeGen.Tests.ToCSharpNameTests.Above<T1>.Below<T1>" )]
        [TestCase( typeof( Nullable<> ), "System.Nullable<T>" )]
        [TestCase( typeof( G<> ), "CK.CodeGen.Tests.ToCSharpNameTests.G<T>" )]
        [TestCase( typeof( H<,> ), "CK.CodeGen.Tests.ToCSharpNameTests.H<T1,T2>" )]
        [TestCase( typeof( Another.I<> ), "CK.CodeGen.Tests.ToCSharpNameTests.Another.I<T3>" )]
        [TestCase( typeof( G<>.Nested ), "CK.CodeGen.Tests.ToCSharpNameTests.G<T>.Nested" )]
        [TestCase( typeof( A<>.C<> ), "CK.CodeGen.Tests.ToCSharpNameTests.A<TB>.C<TD>" )]
        public void ToCSharpName_tests_with_generic_parameter_names( Type type, string expected )
        {
            string actual = type.ToCSharpName( true );
            Assert.AreEqual( expected, actual );
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
