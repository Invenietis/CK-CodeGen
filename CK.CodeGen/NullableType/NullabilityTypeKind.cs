using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen
{
    /// <summary>
    /// Captures nullability information about a Type.
    /// See <see cref="NullabilityTypeExtensions.GetNullabilityKind(Type)"/>.
    /// </summary>
    public enum NullabilityTypeKind : byte
    {
        /// <summary>
        /// Unknown type kind.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Expected nullability flag.
        /// </summary>
        IsNullable = 1,

        /// <summary>
        /// Actual nullability flag: challenging null on a variable of this type should be done.
        /// </summary>
        IsTechnicallyNullable = 2,

        /// <summary>
        /// Value type that may be enclosed in a <see cref="Nullable{T}"/>.
        /// </summary>
        IsValueType = 4,

        /// <summary>
        /// Reference types are <see cref="Type.IsClass"/> or <see cref="Type.IsInterface"/> but NOT <see cref="Type.IsArray"/>.
        /// </summary>
        IsReferenceType = 8,

        /// <summary>
        /// The type is a generic type. For <see cref="Nullable{T}"/>, this applies to the inner T type. 
        /// </summary>
        IsGenericType = 16,

        /// <summary>
        /// The type is a ValueTuple. For <see cref="Nullable{T}"/>, this applies to the inner T type. 
        /// </summary>
        IsTupleType = 32,

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
        /// is necessarily <see cref="IsReferenceType"/> and only <see cref="IsTechnicallyNullable"/> and
        /// if it is a generic type, then all its subordinated types that are reference types are also non nullable reference types.
        /// <para>
        /// This flag can also be set simultaneously with the <see cref="NRTFullNullable"/>: when both are set it means that
        /// the type is marked with a complex NRT NullableAttribute.
        /// Use <see cref="NullablityTypeKindExtension.IsNRTFullNonNullable"/> to test if this type is really NRT nullable.
        /// </para>
        /// </summary>
        NRTFullNonNullable = 128,

        /// <summary>
        /// A nullable value type is <see cref="IsValueType"/>|<see cref="IsNullable"/>|<see cref="IsTechnicallyNullable"/>.
        /// It is wrapped in a <see cref="Nullable{T}"/>.
        /// </summary>
        NullableValueType = IsValueType | IsNullable | IsTechnicallyNullable,

        /// <summary>
        /// A generic value type wrapped in a <see cref="Nullable{T}"/>.
        /// </summary>
        NullableGenericValueType = IsValueType | IsNullable | IsTechnicallyNullable | IsGenericType,

        /// <summary>
        /// A nullable ValueTuple: like <see cref="NullableGenericValueType"/> plus the <see cref="IsTupleType"/>.
        /// </summary>
        NullableTupleType = IsValueType | IsNullable | IsTechnicallyNullable | IsGenericType | IsTupleType,

        /// <summary>
        /// A non nullable value type is only <see cref="IsValueType"/>.
        /// </summary>
        NonNullableValueType = IsValueType,

        /// <summary>
        /// A non nullable generic value type is <see cref="IsValueType"/>|<see cref="IsGenericType"/>.
        /// </summary>
        NonNullableGenericValueType = IsValueType | IsGenericType,

        /// <summary>
        /// A non nullable ValueType is <see cref="IsValueType"/>|<see cref="IsGenericType"/>|<see cref="IsTupleType"/>.
        /// </summary>
        NonNullableTupleType = IsValueType | IsGenericType | IsTupleType,

        /// <summary>
        /// A nullable reference type is <see cref="IsReferenceType"/>|<see cref="IsNullable"/>|<see cref="IsTechnicallyNullable"/>.
        /// </summary>
        NullableReferenceType = IsReferenceType | IsNullable | IsTechnicallyNullable,

        /// <summary>
        /// A nullable generic reference type is <see cref="IsReferenceType"/>|<see cref="IsNullable"/>|<see cref="IsTechnicallyNullable"/>|<see cref="IsGenericType"/>.
        /// </summary>
        NullableGenericReferenceType = IsReferenceType | IsNullable | IsTechnicallyNullable | IsGenericType,

        /// <summary>
        /// A non nullable reference type is <see cref="IsReferenceType"/>|<see cref="IsTechnicallyNullable"/>.
        /// </summary>
        NonNullableReferenceType = IsReferenceType | IsTechnicallyNullable,

        /// <summary>
        /// A non nullable generic reference type is <see cref="IsReferenceType"/>|<see cref="IsTechnicallyNullable"/>>|<see cref="IsGenericType"/>.
        /// </summary>
        NonNullableGenericReferenceType = IsReferenceType | IsTechnicallyNullable | IsGenericType,

    }
}
