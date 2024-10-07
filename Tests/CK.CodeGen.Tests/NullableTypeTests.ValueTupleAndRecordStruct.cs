using CK.Core;
using FluentAssertions;
using FluentAssertions.Equivalency;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace CK.CodeGen.Tests;

public partial class NullableTypeTests
{

    (int?, string?, Action) ValueTuple1 = (1, null, () => { });
    (int?, string?, Action)? ValueTuple2 = null;
    List<(int?, string?, Action)>? ListValueTuple = null;

    (int? X, string? Y, Action) ValueTupleWithName1 = (1, null, () => { });
    (int? X, string? Y, Action)? ValueTupleWithName2 = null;
    List<(int? X, string? Y, Action)>? ListValueTupleWithName = null;

    Dictionary<(int? X, string? Y, Action), ((int? A, int B), string? C, (float, int D))>? DictionaryWithName = null;
    (int? X, string? Y, Action, int? A, int B, string? C, float, int D, double, object E)? LongTupleWithName = null;
    Dictionary<string, ((int? A, int B), string? C, (float, int D))>? DictionaryNoKeyWithName = null;

    // http://mustoverride.com/tuples_names/
    // Element names are semantically insignificant except when used directly.
    [Test]
    public void value_tuple_field_name_erasure()
    {
        var v1 = GetNullableTypeTree( "ValueTuple1" );
        var v2 = GetNullableTypeTree( "ValueTuple2" );
        v1.Kind.IsTupleType().Should().BeTrue();
        v1.Type.Should().BeSameAs( v2.Type );

        var vL = GetNullableTypeTree( "ListValueTuple" );
        vL.RawSubTypes[0].Kind.IsTupleType().Should().BeTrue();
        vL.RawSubTypes[0].Type.Should().BeSameAs( v1.Type );

        var nv1 = GetNullableTypeTree( "ValueTupleWithName1" );
        var nv2 = GetNullableTypeTree( "ValueTupleWithName2" );
        nv1.Kind.IsTupleType().Should().BeTrue();
        nv1.Type.Should().BeSameAs( nv2.Type );

        var nvL = GetNullableTypeTree( "ListValueTupleWithName" );
        nvL.RawSubTypes[0].Kind.IsTupleType().Should().BeTrue();
        nvL.RawSubTypes[0].Type.Should().BeSameAs( nv1.Type );

        nv1.Type.Should().BeSameAs( v1.Type );
    }

    [Test]
    public void value_tuple_field_names_from_generic_parameters_is_breadth_first()
    {
        var fieldInfo = GetType()!.GetField( "ListValueTupleWithName", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic );
        Debug.Assert( fieldInfo != null );
        var attrs = fieldInfo.GetCustomAttributes( false );
        var names = attrs.OfType<TupleElementNamesAttribute>().Single();
        names.TransformNames[0].Should().Be( "X" );
        names.TransformNames[1].Should().Be( "Y" );
        names.TransformNames[2].Should().BeNull();

        fieldInfo = GetType()!.GetField( "DictionaryWithName", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic );
        Debug.Assert( fieldInfo != null );
        attrs = fieldInfo.GetCustomAttributes( false );
        names = attrs.OfType<TupleElementNamesAttribute>().Single();

        //Dictionary<(int? X, string? Y, Action), ((int? A, int B), string? C, (float, int D))>
        names.TransformNames.SequenceEqual( new[]
        {
            "X", "Y", null, // Action has no name
            null, // (int? A, int B) has no name.
            "C",
            null, // (float, int D) has no name.
            "A",
            "B",
            null, // float has no name.
            "D"
        } ).Should().BeTrue();


        fieldInfo = GetType()!.GetField( "LongTupleWithName", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic );
        Debug.Assert( fieldInfo != null );
        attrs = fieldInfo.GetCustomAttributes( false );
        names = attrs.OfType<TupleElementNamesAttribute>().Single();

        // Names are flattened.
        names.TransformNames.SequenceEqual( new[]
        {
            "X",
            "Y",
            null,
            "A",
            "B",
            "C",
            null,
            "D",
            null,
            "E",
            null,
            null,
            null
        } ).Should().BeTrue();

        fieldInfo = GetType()!.GetField( "DictionaryNoKeyWithName", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic );
        Debug.Assert( fieldInfo != null );
        attrs = fieldInfo.GetCustomAttributes( false );
        names = attrs.OfType<TupleElementNamesAttribute>().Single();
        // Only ValueTuples matter.
        // Dictionary<string, ((int? A, int B), string? C, (float,int D))>
        names.TransformNames.SequenceEqual( new[]
        {
            null, // (int? A, int B) has no name.
            "C",
            null, // (float, int D) has no name.
            "A",
            "B",
            null, // float has no name.
            "D"
        } ).Should().BeTrue();


    }

    record struct RecordCanBeEmpty();
    record struct SampleRecord( string Name, (int A, string B) Val );

    [Test]
    public void record_struct_has_no_code_generated_attribute()
    {
        var attrs = typeof( SampleRecord ).GetCustomAttributesData();
        attrs.Should().ContainSingle( a => a.AttributeType.Name == "NullableAttribute"
                                          && a.AttributeType.Namespace == "System.Runtime.CompilerServices" );
        var ctors = typeof( SampleRecord ).GetConstructors();
        ctors.Should().HaveCount( 1 );
        ctors[0].GetCustomAttributes( true ).Should().BeEmpty();
    }

    public interface IPocoLike
    {
        (int A, double? B, List<int>? C)? Tuple { get; }
    }

    public interface IPocoLikeRef
    {
        ref (int A, double? B, List<int>? C)? Tuple { get; }
    }

    [TestCase( "ref" )]
    [TestCase( "val" )]
    public void ref_properties_are_handled_by_NullabilityInfoContext( string mode )
    {
        var pTuple = (mode == "ref" ? typeof( IPocoLikeRef ) : typeof( IPocoLike )).GetProperty( "Tuple" );
        Debug.Assert( pTuple != null );
        pTuple.PropertyType.IsByRef.Should().Be( mode == "ref" );

        var nInfo = (new NullabilityInfoContext()).Create( pTuple );
        CheckCorrectTupleNulabilities( nInfo );
    }

    static void CheckCorrectTupleNulabilities( NullabilityInfo nInfo )
    {
        nInfo.WriteState.Should().Be( NullabilityState.Unknown );
        nInfo.ReadState.Should().Be( NullabilityState.Nullable );
        nInfo.ElementType.Should().BeNull( null );
        nInfo.GenericTypeArguments.Should().HaveCount( 3 );
        nInfo.GenericTypeArguments[0].ReadState.Should().Be( NullabilityState.NotNull );
        nInfo.GenericTypeArguments[0].WriteState.Should().Be( NullabilityState.NotNull );
        nInfo.GenericTypeArguments[1].ReadState.Should().Be( NullabilityState.Nullable );
        nInfo.GenericTypeArguments[1].WriteState.Should().Be( NullabilityState.Nullable );
        nInfo.GenericTypeArguments[2].ReadState.Should().Be( NullabilityState.Nullable );
        nInfo.GenericTypeArguments[2].WriteState.Should().Be( NullabilityState.Nullable );
        nInfo.GenericTypeArguments[2].GenericTypeArguments[0].ReadState.Should().Be( NullabilityState.NotNull );
        nInfo.GenericTypeArguments[2].GenericTypeArguments[0].WriteState.Should().Be( NullabilityState.NotNull );
    }

    [TestCase( "ref" )]
    [TestCase( "val" )]
    public void the_TEMPNullabilityInfo_must_be_temporarily_used( string mode )
    {
        var pTuple = (mode == "ref" ? typeof( IPocoLikeRef ) : typeof( IPocoLike )).GetProperty( "Tuple" );
        Debug.Assert( pTuple != null );
        pTuple.PropertyType.IsByRef.Should().Be( mode == "ref" );

        var nInfo = (new TEMPNullabilityInfoContext()).Create( pTuple );
        CheckCorrectTupleNulabilities( nInfo );
    }

    static void CheckCorrectTupleNulabilities( TEMPNullabilityInfo nInfo )
    {
        nInfo.WriteState.Should().Be( NullabilityState.Unknown );
        nInfo.ReadState.Should().Be( NullabilityState.Nullable );
        nInfo.ElementType.Should().BeNull( null );
        nInfo.GenericTypeArguments.Should().HaveCount( 3 );
        nInfo.GenericTypeArguments[0].ReadState.Should().Be( NullabilityState.NotNull );
        nInfo.GenericTypeArguments[0].WriteState.Should().Be( NullabilityState.NotNull );
        nInfo.GenericTypeArguments[1].ReadState.Should().Be( NullabilityState.Nullable );
        nInfo.GenericTypeArguments[1].WriteState.Should().Be( NullabilityState.Nullable );
        nInfo.GenericTypeArguments[2].ReadState.Should().Be( NullabilityState.Nullable );
        nInfo.GenericTypeArguments[2].WriteState.Should().Be( NullabilityState.Nullable );
        nInfo.GenericTypeArguments[2].GenericTypeArguments[0].ReadState.Should().Be( NullabilityState.NotNull );
        nInfo.GenericTypeArguments[2].GenericTypeArguments[0].WriteState.Should().Be( NullabilityState.NotNull );
    }
}
