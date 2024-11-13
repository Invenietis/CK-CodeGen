using System;

namespace CK.CodeGen;

/// <summary>
/// Captures nullability information about a Type.
/// See <see cref="NullabilityTypeExtensions.GetNullabilityKind(Type)"/>.
/// </summary>
[Flags]
public enum NullabilityTypeKind : int
{
    /// <summary>
    /// Unknown type kind.
    /// </summary>
    None = 0,

    /// <summary>
    /// Expected nullability flag.
    /// </summary>
    IsNullable = 1,

    /// <summary>
    /// Value type that may be enclosed in a <see cref="Nullable{T}"/>.
    /// </summary>
    IsValueType = 2,

    /// <summary>
    /// Reference types are <see cref="Type.IsClass"/> or <see cref="Type.IsInterface"/>.
    /// </summary>
    IsReferenceType = 4,

    /// <summary>
    /// The type is a generic type. For <see cref="Nullable{T}"/>, this applies to the inner T type. 
    /// </summary>
    IsGenericType = 8,

    /// <summary>
    /// The type is a ValueTuple. For <see cref="Nullable{T}"/>, this applies to the inner T type. 
    /// </summary>
    IsTupleType = 16,

    /// <summary>
    /// The type is a generic type parameter. 
    /// </summary>
    IsGenericParameter = 32,

    /// <summary>
    /// Optional flag that describes a Nullable Reference Type marked with NullableAttribute(2): the type
    /// is necessarily <see cref="IsReferenceType"/> and <see cref="IsNullable"/> and if the type has generic arguments, then
    /// all its subordinated types that are reference types are also nullable.
    /// <para>
    /// This flag can also be set simultaneously with the <see cref="NRTFullNonNullable"/>: when both are set it means that
    /// the type is marked with a complex NRT NullableAttribute.
    /// Use <see cref="NullablityTypeKindExtension.IsNRTFullNullable"/> to test if this type is really NRT full nullable.
    /// </para>
    /// </summary>
    NRTFullNullable = 64,

    /// <summary>
    /// Optional flag that describes a Nullable Reference Type marked with NullableAttribute(1): the type
    /// is necessarily <see cref="IsReferenceType"/> and if it is a generic type, then all its subordinated types
    /// that are reference types are also non nullable reference types (without considering the notnull generic constraint
    /// that I failed to handle).
    /// <para>
    /// This flag can also be set simultaneously with the <see cref="NRTFullNullable"/>: when both are set it means that
    /// the type is marked with a complex NRT NullableAttribute.
    /// Use <see cref="NullablityTypeKindExtension.IsNRTFullNonNullable"/> to test if this type is really NRT nullable.
    /// </para>
    /// </summary>
    NRTFullNonNullable = 128,

    /// <summary>
    /// A nullable value type is <see cref="IsValueType"/>|<see cref="IsNullable"/>.
    /// It is wrapped in a <see cref="Nullable{T}"/>.
    /// </summary>
    NullableValueType = IsValueType | IsNullable,

    /// <summary>
    /// A generic value type wrapped in a <see cref="Nullable{T}"/>.
    /// </summary>
    NullableGenericValueType = IsValueType | IsNullable | IsGenericType,

    /// <summary>
    /// A nullable ValueTuple: like <see cref="NullableGenericValueType"/> plus the <see cref="IsTupleType"/>.
    /// </summary>
    NullableTupleType = IsValueType | IsNullable | IsGenericType | IsTupleType,

    /// <summary>
    /// A non nullable generic value type is <see cref="IsValueType"/>|<see cref="IsGenericType"/>.
    /// </summary>
    NonNullableGenericValueType = IsValueType | IsGenericType,

    /// <summary>
    /// A non nullable ValueType is <see cref="IsValueType"/>|<see cref="IsGenericType"/>|<see cref="IsTupleType"/>.
    /// </summary>
    NonNullableTupleType = IsValueType | IsGenericType | IsTupleType,

    /// <summary>
    /// A nullable reference type is <see cref="IsReferenceType"/>|<see cref="IsNullable"/>.
    /// </summary>
    NullableReferenceType = IsReferenceType | IsNullable,

    /// <summary>
    /// A nullable generic reference type is <see cref="IsReferenceType"/>|<see cref="IsNullable"/>|<see cref="IsGenericType"/>.
    /// </summary>
    NullableGenericReferenceType = IsReferenceType | IsNullable | IsGenericType,

    /// <summary>
    /// A non nullable reference type is <see cref="IsReferenceType"/>.
    /// </summary>
    NonNullableReferenceType = IsReferenceType,

    /// <summary>
    /// A non nullable generic reference type is <see cref="IsReferenceType"/>|<see cref="IsGenericType"/>.
    /// </summary>
    NonNullableGenericReferenceType = IsReferenceType | IsGenericType,

}
