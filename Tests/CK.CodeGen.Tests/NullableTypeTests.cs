using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CK.CodeGen.Tests
{
    [TestFixture]
    partial class NullableTypeTests
    {
        [Test]
        public void Type_from_typeof_is_oblivious_to_nullable()
        {
            var t1 = typeof( List<string?> );
            var t2 = typeof( List<string> );

            t1.Should().BeSameAs( t2 );

            var a = t1.GetCustomAttributesData().Single( a => a.AttributeType.Name == "NullableAttribute" && a.AttributeType.Namespace == "System.Runtime.CompilerServices" );
            object? data = a.ConstructorArguments[0].Value;
            // A single value means "apply to everything in the type", e.g. 1 for Dictionary<string, string>, 2 for Dictionary<string?, string?>?
            // Here, we have a single 0 (oblivious).
            data.Should().Be( (byte)0 );

            var n2 = t2.GetNullableTypeTree();
            n2.ToString().Should().Be( "List<string?>?", "The basic extension methods (oblivious context): all reference types are nullable." );

            // This is the same as a null NRT profile.
            var n1 = t1.GetNullableTypeTree( new NullabilityTypeInfo( t1.GetNullabilityKind(), null ) );
            n1.ToString().Should().Be( "List<string?>?" );

            n1.Equals( n2 ).Should().BeTrue();
            n1.GetHashCode().Should().Be( n2.GetHashCode() );
        }

#pragma warning disable IDE0052 // Remove unread private members
#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE1006 // Naming Styles

        List<int> ListInt { get; } = new List<int>();

        List<int?> ListNInt { get; } = new List<int?>();

        List<int?>? NListNInt { get; } = new List<int?>();

        List<string> ListString = new List<string>();

        List<string?> ListNString = new List<string?>();

        List<string?>? NListNString = new List<string?>();

        List<(int, string)> ListValueTupleIntString = new List<(int, string)>();

        List<(int?, string?)?>? NListNValueTupleNIntNString = new List<(int?, string?)?>();

        List<(int, string?)>? NListValueTupleIntNString = new List<(int, string?)>();

        List<(int, string?)?>? NListNValueTupleIntNString = new List<(int, string?)?>();

        List<(int, string, int, string)> ListValueTupleIntStringIntString = new List<(int, string, int, string)>();

        List<((int, string), int, (int,string))> ListValueTupleVTIntStringIntVTIntString = new List<((int, string), int, (int, string))>();

        List<(((int, int), (int, int)), ((int, int), (int, int)))> ListVT = new List<(((int, int), (int, int)), ((int, int), (int, int)))>();

        List<(((int, int), (int, string?)), ((int, int), (int, int)))> ListVTOneNString = new List<(((int, int), (int, string?)), ((int, int), (int, int)))>();

        List<List<List<List<Attribute>?>>?> ListNListListNListAttr = new List<List<List<List<Attribute>?>>?>();

        List<List<List<List<Attribute>?>>?>? NListNListListNListAttr = new List<List<List<List<Attribute>?>>?>();

        [TestCase( "ListInt", "List<int>", "NonNullableGenericReferenceType (NRT:FullNonNull)" )]
        [TestCase( "ListNInt", "List<int?>", "NonNullableGenericReferenceType (NRT:FullNonNull)" )]
        [TestCase( "NListNInt", "List<int?>?", "NullableGenericReferenceType (NRT:FullNull)" )]
        [TestCase( "ListString", "List<string>", "NonNullableGenericReferenceType (NRT:FullNonNull)" )]
        [TestCase( "ListNString", "List<string?>", "NonNullableGenericReferenceType (NRT:Profile) - 2" )]
        [TestCase( "NListNString", "List<string?>?", "NullableGenericReferenceType (NRT:FullNull)" )]
        [TestCase( "ListValueTupleIntString", "List<(int,string)>", "NonNullableGenericReferenceType (NRT:Profile) - 01" )]
        [TestCase( "NListNValueTupleNIntNString", "List<(int?,string?)?>?", "NullableGenericReferenceType (NRT:Profile) - 02" )]
        [TestCase( "NListValueTupleIntNString", "List<(int,string?)>?", "NullableGenericReferenceType (NRT:Profile) - 02" )]
        [TestCase( "NListNValueTupleIntNString", "List<(int,string?)?>?", "NullableGenericReferenceType (NRT:Profile) - 02" )]
        [TestCase( "ListValueTupleIntStringIntString", "List<(int,string,int,string)>", "NonNullableGenericReferenceType (NRT:Profile) - 011" )]
        [TestCase( "ListValueTupleVTIntStringIntVTIntString", "List<((int,string),int,(int,string))>", "NonNullableGenericReferenceType (NRT:Profile) - 00101" )]
        [TestCase( "ListVT", "List<(((int,int),(int,int)),((int,int),(int,int)))>", "NonNullableGenericReferenceType (NRT:Profile) - 0000000" )]
        [TestCase( "ListVTOneNString", "List<(((int,int),(int,string?)),((int,int),(int,int)))>", "NonNullableGenericReferenceType (NRT:Profile) - 00002000" )]
        [TestCase( "ListNListListNListAttr", "List<List<List<List<Attribute>?>>?>", "NonNullableGenericReferenceType (NRT:Profile) - 2121" )]
        [TestCase( "NListNListListNListAttr", "List<List<List<List<Attribute>?>>?>?", "NullableGenericReferenceType (NRT:Profile) - 2121" )]
        public void List_NullableTypeTree( string member, string result, string info )
        {
            CheckAll( member, result, info );
        }


#pragma warning disable 169, 414  // The field is never used, The field is assigned but its value is never used

        (int, string) VTIntString = (1, "");

        (int, string, int?, Action)? NVTIntStringNIntAction = null;

        (int, int) VTIntInt = (1, 1);

        (int, string?)? NVTIntNString = null;

        (int, string?, string, string?)? NVTIntNStringStringNString = null;

        ((int, string), (string, string))? NVTVTIntStringVTStringString = null;

        void FromParameter( (string, List<string>, int) p ) { }

        [TestCase( "VTIntString", "(int,string)", "NonNullableTupleType (NRT:Profile) - 1" )]
        [TestCase( "NVTIntStringNIntAction", "(int,string,int?,Action)?", "NullableTupleType (NRT:Profile) - 11" )]
        [TestCase( "VTIntInt", "(int,int)", "NonNullableTupleType" )]
        [TestCase( "NVTIntNString", "(int,string?)?", "NullableTupleType (NRT:Profile) - 2" )]
        [TestCase( "NVTIntNStringStringNString", "(int,string?,string,string?)?", "NullableTupleType (NRT:Profile) - 212" )]
        [TestCase( "NVTVTIntStringVTStringString", "((int,string),(string,string))?", "NullableTupleType (NRT:Profile) - 01011" )]
        [TestCase( "FromParameter", "(string,List<string>,int)", "NonNullableTupleType (NRT:Profile) - 111" )]
        public void ValueTuple_NullableTypeTree( string member, string result, string info )
        {
            CheckAll( member, result, info );
        }

        Dictionary<List<string?>, string[]?> Example00 = new Dictionary<List<string?>, string[]?>();

        Dictionary<List<string?>, string[]> Example01 = new Dictionary<List<string?>, string[]>();

        ISet<IDictionary<string,LinkedList<Func<string?,string,Func<Action<ISet<Action>>>>>>>? Example02() => null;

        [TestCase( "Example00", "Dictionary<List<string?>,string[]?>", "NonNullableGenericReferenceType (NRT:Profile) - 1221" )]
        [TestCase( "Example01", "Dictionary<List<string?>,string[]>", "NonNullableGenericReferenceType (NRT:Profile) - 1211" )]
        [TestCase( "Example02", "ISet<IDictionary<string,LinkedList<Func<string?,string,Func<Action<ISet<Action>>>>>>>?", "NullableGenericReferenceType (NRT:Profile) - 1111211111" )]
        public void Example_NullableTypeTree( string member, string result, string info )
        {
            CheckAll( member, result, info );
        }

        int VT;

        int? NVT;

        [TestCase( "VT", "int", "NonNullableValueType" )]
        [TestCase( "NVT", "int?", "NullableValueType" )]
        public void NonNullable_at_all_types( string member, string result, string info )
        {
            CheckAll( member, result, info );
        }

        class GenParent<T>
        {
            public class NestedNotSupported
            {
            }

            public class LevelDoesNotMatter
            {
                public class NestedNotSupported
                {
                }
            }
        }

        class NotGeneric : GenParent<int>
        {
            public class NestedSupported
            {
            }

            public class LevelDoesNotMatter2
            {
                public class NestedSupported
                {
                }
            }
        }

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable 0649 // Field 'is never assigned to, and will always have its default value null
        
        GenParent<int>.NestedNotSupported NotSupported;

        NotGeneric.NestedNotSupported AlsoNotSupported;

        NotGeneric.NestedSupported Supported;

        ISet<NotGeneric.NestedSupported?> SupportedInside;

        GenParent<int>.LevelDoesNotMatter.NestedNotSupported NotSupportedInner;

        NotGeneric.LevelDoesNotMatter2.NestedSupported SupportedInner;

        Dictionary<NotGeneric.LevelDoesNotMatter2.NestedSupported, NotGeneric.NestedSupported?> SupportedInnerInside;

        [Test]
        public void NullableTypeTree_throws_on_generic_nesting_type()
        {
            var ns = GetTypeAndNullability( nameof( NotSupported ) );
            ns.Invoking( sut => sut.Type.GetNullableTypeTree( ns.Nullability ) ).Should().Throw<ArgumentException>().WithMessage( "*Only nested types in non generic types are supported.*" );

            var nsA = GetTypeAndNullability( nameof( AlsoNotSupported ) );
            nsA.Invoking( sut => sut.Type.GetNullableTypeTree( nsA.Nullability ) ).Should().Throw<ArgumentException>().WithMessage( "*Only nested types in non generic types are supported.*" );

            var nsi = GetTypeAndNullability( nameof( NotSupportedInner ) );
            nsi.Invoking( sut => sut.Type.GetNullableTypeTree( nsi.Nullability ) ).Should().Throw<ArgumentException>().WithMessage( "*Only nested types in non generic types are supported.*" );

            var s = GetTypeAndNullability( nameof( Supported ) );
            s.Invoking( sut => sut.Type.GetNullableTypeTree( s.Nullability ) ).Should().NotThrow();

            var si = GetTypeAndNullability( nameof( SupportedInner ) );
            si.Invoking( sut => sut.Type.GetNullableTypeTree( si.Nullability ) ).Should().NotThrow();
        }

        [Test]
        public void NullableTypeTree_ToString_can_include_namespaces_and_non_generic_nesting_type_names()
        {
            {
                var s = GetTypeAndNullability( nameof( Supported ) );
                var t = s.Type.GetNullableTypeTree( s.Nullability );
                t.ToString( true ).Should().Be( "CK.CodeGen.Tests.NullableTypeTests.NotGeneric.NestedSupported" );
            }
            {
                var s = GetTypeAndNullability( nameof( SupportedInner ) );
                var t = s.Type.GetNullableTypeTree( s.Nullability );
                t.ToString( true ).Should().Be( "CK.CodeGen.Tests.NullableTypeTests.NotGeneric.LevelDoesNotMatter2.NestedSupported" );
            }
            {
                var s = GetTypeAndNullability( nameof( SupportedInside ) );
                var t = s.Type.GetNullableTypeTree( s.Nullability );
                t.ToString( true ).Should().Be( "System.Collections.Generic.ISet<CK.CodeGen.Tests.NullableTypeTests.NotGeneric.NestedSupported?>" );
            }
            {
                var s = GetTypeAndNullability( nameof( SupportedInnerInside ) );
                var t = s.Type.GetNullableTypeTree( s.Nullability );
                t.ToString( true ).Should().Be( "System.Collections.Generic.Dictionary<CK.CodeGen.Tests.NullableTypeTests.NotGeneric.LevelDoesNotMatter2.NestedSupported,CK.CodeGen.Tests.NullableTypeTests.NotGeneric.NestedSupported?>" );
            }
        }


        void CheckAll( string member, string result, string info )
        {
            var n = GetTypeAndNullability( member );
            n.Nullability.ToString().Should().Be( info );

            var t = n.Type.GetNullableTypeTree( n.Nullability );
            t.ToString().Should().Be( result );
        }

        [DebuggerStepThrough]
        private (Type Type, NullabilityTypeInfo Nullability) GetTypeAndNullability( string name )
        {
            var p = GetType()!.GetProperty( name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic );
            if( p != null ) return (p.PropertyType, p.GetNullabilityInfo());
            var f = GetType()!.GetField( name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic );
            if( f != null ) return (f!.FieldType, f!.GetNullabilityInfo());

            var m = GetType()!.GetMethod( name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic );
            Debug.Assert( m != null );
            var parameter = m.ReturnType != typeof( void ) ? m.ReturnParameter : m.GetParameters()[0];
            return (parameter.ParameterType, parameter.GetNullabilityInfo()); 
        }


    }
}
