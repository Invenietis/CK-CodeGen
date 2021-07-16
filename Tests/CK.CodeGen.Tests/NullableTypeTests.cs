using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace CK.CodeGen.Tests
{
    [TestFixture]
    partial class NullableTypeTests
    {
        [TestCase( "NonNullableRef" )]
        [TestCase( "Ref" )]
        [TestCase( "NullableValueType" )]
        [TestCase( "ValueType" )]
        public void Normal_and_Abnormal_null( string kind )
        {
            switch( kind )
            {
                case "NonNullableRef":
                    {
                        var tRef = GetNullableTypeTree( nameof( ListInt ) );
                        tRef.Kind.IsNullable().Should().BeFalse();

                        var tNormal = tRef.ToNormalNull();
                        tNormal.Kind.IsNullable().Should().BeTrue();
                        tNormal.Should().NotBe( tRef );

                        var tAbnormal = tRef.ToAbnormalNull();
                        tAbnormal.Should().Be( tRef );

                        CheckMirror( tNormal, tAbnormal );
                        break;
                    }
                case "Ref":
                    {
                        var t = GetNullableTypeTree( nameof( NListNInt ) );
                        t.Kind.IsNullable().Should().BeTrue();

                        var tNormal = t.ToNormalNull();
                        tNormal.Should().Be( t );

                        var tAbnormal = t.ToAbnormalNull();
                        tAbnormal.Kind.IsNullable().Should().BeFalse();
                        tAbnormal.Should().NotBe( t );

                        CheckMirror( tNormal, tAbnormal );
                        break;
                    }
                case "NullableValueType":
                    {
                        var t = GetNullableTypeTree( nameof( NVT ) );
                        t.Kind.IsNullable().Should().BeTrue();

                        var tNormal = t.ToNormalNull();
                        tNormal.Should().NotBe( t );

                        var tAbnormal = t.ToAbnormalNull();
                        tAbnormal.Kind.IsNullable().Should().BeTrue();
                        tAbnormal.Should().Be( t );

                        CheckMirror( tNormal, tAbnormal );
                        break;
                    }
                case "ValueType":
                    {
                        var t = GetNullableTypeTree( nameof( VT ) );
                        t.Kind.IsNullable().Should().BeFalse();

                        var tNormal = t.ToNormalNull();
                        tNormal.Should().Be( t );

                        var tAbnormal = t.ToAbnormalNull();
                        tAbnormal.Kind.IsNullable().Should().BeTrue();
                        tAbnormal.Should().NotBe( t );

                        CheckMirror( tNormal, tAbnormal );
                        break;
                    }
                default: throw new NotSupportedException();
            }
            static void CheckMirror( NullableTypeTree normal, NullableTypeTree abnormal )
            {
                normal.ToAbnormalNull().ToNormalNull().Should().Be( normal );
                abnormal.ToNormalNull().ToAbnormalNull().Should().Be( abnormal );
            }
        }


        [Test]
        public void Type_from_typeof_declaration_is_oblivious_to_nullable()
        {
            var t1 = typeof( List<string?> );
            var t2 = typeof( List<string> );

            t1.Should().BeSameAs( t2, "No difference at all, same reference... :(" );

            var a = t1.CustomAttributes.Single( a => a.AttributeType.Name == "NullableAttribute" && a.AttributeType.Namespace == "System.Runtime.CompilerServices" );
            object? data = a.ConstructorArguments[0].Value;
            // A single value means "apply to everything in the type", e.g. 1 for Dictionary<string, string>, 2 for Dictionary<string?, string?>?
            // Here, we have a single 0 (oblivious).
            data.Should().Be( (byte)0 );

            var aCtx = t1.CustomAttributes.Single( a => a.AttributeType.Name == "NullableContextAttribute" && a.AttributeType.Namespace == "System.Runtime.CompilerServices" );
            aCtx.ConstructorArguments[0].Value.Should().Be( (byte)1, "The type is a also marked with NullableContextAttribute(1). Why?" );

            var pA = t1.GetGenericArguments()[0].CustomAttributes.Single( a => a.AttributeType.Name == "NullableAttribute" && a.AttributeType.Namespace == "System.Runtime.CompilerServices" );
            pA.ConstructorArguments[0].Value.Should().Be( (byte)0, "The <string> parameter is marked with NullableAttribute(0)..." );
            var pACtx = t1.GetGenericArguments()[0].CustomAttributes.Single( a => a.AttributeType.Name == "NullableContextAttribute" && a.AttributeType.Namespace == "System.Runtime.CompilerServices" );
            pACtx.ConstructorArguments[0].Value.Should().Be( (byte)1, "...and also with a NullableContextAttribute(1)." );

            var n2 = t2.GetNullableTypeTree();
            n2.ToString().Should().Be( "List<string?>?", "The basic extension methods (oblivious context): all reference types are nullable." );

            // This is the same as a null NRT profile (every reference types are nullable).
            NullableTypeTree n1 = t1.GetNullableTypeTree( new NullabilityTypeInfo( t1.GetNullabilityKind(), null ) );
            n1.ToString().Should().Be( "List<string?>?" );

            n1.Equals( n2 ).Should().BeTrue( "NullableTypeTree has a value semantics." );
            n1.GetHashCode().Should().Be( n2.GetHashCode() );
        }


        interface IDic<TKey, TValue> {}

        interface IDicNKey<TKey, TValue> where TKey : notnull { }

        interface IDicNValue<TKey, TValue> where TValue : notnull { }

        interface IDicNKeyNValue<TKey, TValue> where TKey : notnull where TValue : notnull { }

        [Test]
        public void WithSubTypeAt_updates_SubTypes()
        {
            var s = GetTypeAndNullability( nameof( VeryLongValueTuple ) );
            var t = s.Type.GetNullableTypeTree( s.Nullability );
            t.ToString().Should().Be( "(sbyte,byte,short,ushort,int,uint,long,ulong,decimal,BigInteger,IEnumerable<int>,string?,List<string?>?,sbyte?,byte?,short?,ushort?,int?,uint?,long?,ulong?,decimal?)" );
            t.SubTypes.ElementAt( 0 ).ToString().Should().Be( "sbyte" );
            for(int idx = 0; idx < t.SubTypes.Count(); ++idx )
            {
                var t1 = t.WithSubTypeAt( idx, GetType().GetNullableTypeTree() );
                t1.SubTypes.ElementAt( idx ).ToString().Should().Be( "NullableTypeTests?" );
            }
        }


        [Test]
        public void research_on_notnull_generic_constraint()
        {
            // No constraint at all.
            {
                var t = typeof( IDic<,> );
                t.GetCustomAttributesData()
                        .FirstOrDefault( a => a.AttributeType.Name == "NullableAttribute" && a.AttributeType.Namespace == "System.Runtime.CompilerServices" )
                        .Should().BeNull();

                var a = t.GetCustomAttributesData()
                        .FirstOrDefault( a => a.AttributeType.Name == "NullableContextAttribute" && a.AttributeType.Namespace == "System.Runtime.CompilerServices" );
                object? data = a.ConstructorArguments[0].Value;
                data.Should().Be( (byte)2 );
            }
            // TKey notnull.
            {
                var t = typeof( IDicNKey<,> );
                t.GetCustomAttributesData()
                        .FirstOrDefault( a => a.AttributeType.Name == "NullableAttribute" && a.AttributeType.Namespace == "System.Runtime.CompilerServices" )
                        .Should().BeNull();

                t.GetCustomAttributesData()
                        .FirstOrDefault( a => a.AttributeType.Name == "NullableContextAttribute" && a.AttributeType.Namespace == "System.Runtime.CompilerServices" )
                        .Should().BeNull();

                // Should be the same as IDictionary... but it's not!
                // It's marked with context 1.
                var tDic = typeof( IDictionary<,> );
                tDic.GetCustomAttributesData()
                        .FirstOrDefault( a => a.AttributeType.Name == "NullableAttribute" && a.AttributeType.Namespace == "System.Runtime.CompilerServices" )
                        .Should().BeNull();

                var a = tDic.GetCustomAttributesData()
                        .FirstOrDefault( a => a.AttributeType.Name == "NullableContextAttribute" && a.AttributeType.Namespace == "System.Runtime.CompilerServices" );
                object? data = a.ConstructorArguments[0].Value;
                data.Should().Be( (byte)1 );
            }
            // TValue notnull.
            {
                var t = typeof( IDicNValue<,> );
                t.GetCustomAttributesData()
                        .FirstOrDefault( a => a.AttributeType.Name == "NullableAttribute" && a.AttributeType.Namespace == "System.Runtime.CompilerServices" )
                        .Should().BeNull();

                var a = t.GetCustomAttributesData()
                        .FirstOrDefault( a => a.AttributeType.Name == "NullableContextAttribute" && a.AttributeType.Namespace == "System.Runtime.CompilerServices" )
                        .Should().BeNull();
            }
            // TKey and TValue notnull.
            {
                var t = typeof( IDicNKeyNValue<,> );
                t.GetCustomAttributesData()
                        .FirstOrDefault( a => a.AttributeType.Name == "NullableAttribute" && a.AttributeType.Namespace == "System.Runtime.CompilerServices" )
                        .Should().BeNull();

                t.GetCustomAttributesData()
                  .FirstOrDefault( a => a.AttributeType.Name == "NullableContextAttribute" && a.AttributeType.Namespace == "System.Runtime.CompilerServices" )
                  .Should().BeNull();

            }
        }

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable 0649 // Field 'is never assigned to, and will always have its default value null
#pragma warning disable IDE1006 // Naming Styles

        List<int> ListInt { get; } = new();

        List<int?> ListNInt { get; } = new();

        List<int?>? NListNInt { get; } = new();

        List<string> ListString = new();

        List<string?> ListNString = new();

        List<string?>? NListNString = new();

        List<(int, string)> ListValueTupleIntString = new();

        List<(int?, string?)?>? NListNValueTupleNIntNString = new();

        List<(int, string?)>? NListValueTupleIntNString = new();

        List<(int, string?)?>? NListNValueTupleIntNString = new();

        List<(int, string, int, string)> ListValueTupleIntStringIntString = new();

        List<((int, string), int, (int,string))> ListValueTupleVTIntStringIntVTIntString = new();

        List<(((int, int), (int, int)), ((int, int), (int, int)))> ListVT = new();

        List<(((int, int), (int, string?)), ((int, int), (int, int)))> ListVTOneNString = new();

        List<List<List<List<Attribute>?>>?> ListNListListNListAttr = new();

        List<List<List<List<Attribute>?>>?>? NListNListListNListAttr = new();

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

        [TestCase( "ListString", "ListNString" )]
        [TestCase( "ListString", "NListNString" )]
        [TestCase( "ListNListListNListAttr", "NListNListListNListAttr" )]
        public void Types_are_equals_when_only_reference_types_are_used( string m1, string m2 )
        {
            var (type1, info1) = GetTypeAndNullability( m1 );
            var n1 = type1.GetNullableTypeTree( info1 );

            var (type2, info2) = GetTypeAndNullability( m2 );
            var n2 = type2.GetNullableTypeTree( info2 );

            n1.Should().NotBe( n2, "We handle nullability profile." );
            type1.Should().BeSameAs( type2 );
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

        Dictionary<List<string?>, string[]?> Example00 = new();

        Dictionary<List<string?>, string[]> Example01 = new();

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


        (sbyte, byte, short, ushort, int, uint, long, ulong, decimal, System.Numerics.BigInteger, string) LongValueTuple1 { get; }
        (sbyte, byte, short, ushort, int, uint, long, (ulong, decimal, System.Numerics.BigInteger)) LongValueTuple2 { get; }
        (sbyte, byte, (short, ushort, int), uint, long, ulong, decimal, System.Numerics.BigInteger) LongValueTuple3 { get; }

        [Test]
        public void handling_long_value_tuples_is_not_simple()
        {
            var s1 = GetTypeAndNullability( nameof( LongValueTuple1 ) );
            var t1 = s1.Type.GetNullableTypeTree( s1.Nullability );

            var s2 = GetTypeAndNullability( nameof( LongValueTuple2 ) );
            var t2 = s2.Type.GetNullableTypeTree( s2.Nullability );

            var s3 = GetTypeAndNullability( nameof( LongValueTuple3 ) );
            var t3 = s3.Type.GetNullableTypeTree( s3.Nullability );

            t1.Should().NotBe( t2 );

            t1.IsLongValueTuple.Should().BeTrue();
            t1.RawSubTypes.Should().HaveCount( 8 );
            t1.RawSubTypes[^1].Type.Name.Should().Be( "ValueTuple`4" );
            t1.SubTypes.Should().HaveCount( 11, "SubTypes lifts the 8th ValueTuple." );
            t1.ToString().Should().Be( "(sbyte,byte,short,ushort,int,uint,long,ulong,decimal,BigInteger,string)" );

            t2.IsLongValueTuple.Should().BeTrue();
            t2.RawSubTypes.Count.Should().Be( 8 );
            t2.RawSubTypes[^1].Type.Name.Should().Be( "ValueTuple`1", "The last 8th possible index, is wrapped in a ValueTuple<T> ('singleton' tuple)." );
            t2.RawSubTypes[^1].RawSubTypes[0].Type.Name.Should().Be( "ValueTuple`3" );
            t2.SubTypes.Should().HaveCount( 8 );
            t2.ToString().Should().Be( "(sbyte,byte,short,ushort,int,uint,long,(ulong,decimal,BigInteger))" );

            t3.RawSubTypes.Count.Should().Be( 8 );
            t3.RawSubTypes[2].Type.Name.Should().Be( "ValueTuple`3" );
            t3.IsLongValueTuple.Should().BeTrue();
            t3.SubTypes.Should().HaveCount( 8 );
            t3.RawSubTypes.Last().Type.Name.Should().Be( "ValueTuple`1" );
            t3.SubTypes.Last().Type.Should().Be( typeof( System.Numerics.BigInteger ) );
            t3.ToString().Should().Be( "(sbyte,byte,(short,ushort,int),uint,long,ulong,decimal,BigInteger)" );

        }

        (sbyte, byte, short, ushort, int, uint, long, ulong, decimal, System.Numerics.BigInteger, IEnumerable<int>, string?, List<string?>?, sbyte?, byte?, short?, ushort?, int?, uint?, long?, ulong?, decimal?) VeryLongValueTuple { get; }

        [Test]
        public void handling_very_very_long_value_tuples_works()
        {
            var s = GetTypeAndNullability( nameof( VeryLongValueTuple ) );
            var t = s.Type.GetNullableTypeTree( s.Nullability );

            t.IsLongValueTuple.Should().BeTrue();
            t.RawSubTypes.Should().HaveCount( 8 );
            t.SubTypes.Should().HaveCount( 22 );
            t.ToString().Should().Be( "(sbyte,byte,short,ushort,int,uint,long,ulong,decimal,BigInteger,IEnumerable<int>,string?,List<string?>?,sbyte?,byte?,short?,ushort?,int?,uint?,long?,ulong?,decimal?)" );

            var rawType = s.Type;
            rawType.ToCSharpName().Should().Be( "(sbyte,byte,short,ushort,int,uint,long,ulong,decimal,System.Numerics.BigInteger,System.Collections.Generic.IEnumerable<int>,string,System.Collections.Generic.List<string>,sbyte?,byte?,short?,ushort?,int?,uint?,long?,ulong?,decimal?)" );
            rawType.ToCSharpName( useValueTupleParentheses: false ).Should().Be( "System.ValueTuple<sbyte,byte,short,ushort,int,uint,long,System.ValueTuple<ulong,decimal,System.Numerics.BigInteger,System.Collections.Generic.IEnumerable<int>,string,System.Collections.Generic.List<string>,sbyte?,System.ValueTuple<byte?,short?,ushort?,int?,uint?,long?,ulong?,System.ValueTuple<decimal?>>>>" );
        }

        [Test]
        public void long_value_tuples_ToCSharpName_works()
        {
            var t1 = GetTypeAndNullability( nameof( LongValueTuple1 ) ).Type;
            var t2 = GetTypeAndNullability( nameof( LongValueTuple2 ) ).Type;
            var t3 = GetTypeAndNullability( nameof( LongValueTuple3 ) ).Type;
            t1.ToCSharpName().Should().Be( "(sbyte,byte,short,ushort,int,uint,long,ulong,decimal,System.Numerics.BigInteger,string)" );
            t2.ToCSharpName().Should().Be( "(sbyte,byte,short,ushort,int,uint,long,(ulong,decimal,System.Numerics.BigInteger))" );
            t3.ToCSharpName().Should().Be( "(sbyte,byte,(short,ushort,int),uint,long,ulong,decimal,System.Numerics.BigInteger)" );
            t1.ToCSharpName( useValueTupleParentheses: false ).Should().Be( "System.ValueTuple<sbyte,byte,short,ushort,int,uint,long,System.ValueTuple<ulong,decimal,System.Numerics.BigInteger,string>>" );
            t2.ToCSharpName( useValueTupleParentheses: false ).Should().Be( "System.ValueTuple<sbyte,byte,short,ushort,int,uint,long,System.ValueTuple<System.ValueTuple<ulong,decimal,System.Numerics.BigInteger>>>" );
            t3.ToCSharpName( useValueTupleParentheses: false ).Should().Be( "System.ValueTuple<sbyte,byte,System.ValueTuple<short,ushort,int>,uint,long,ulong,decimal,System.ValueTuple<System.Numerics.BigInteger>>" );
        }

        void CheckAll( string member, string result, string info )
        {
            var n = GetTypeAndNullability( member );
            n.Nullability.ToString().Should().Be( info );

            var t = n.Type.GetNullableTypeTree( n.Nullability );
            t.ToString().Should().Be( result );
        }

        [DebuggerStepThrough]
        (Type Type, NullabilityTypeInfo Nullability) GetTypeAndNullability( string name )
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

        [DebuggerStepThrough]
        NullableTypeTree GetNullableTypeTree( string name )
        {
            var s = GetTypeAndNullability( name );
            return s.Type.GetNullableTypeTree( s.Nullability );
        }


    }
}
