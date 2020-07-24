using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen
{
    /// <summary>
    /// Captures nullability information about a Type.
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
        /// Optional flag thar describes a Nullable Reference Type marked with NullableAttribute(2): the type
        /// is necessarily <see cref="IsReferenceType"/> and <see cref="IsNullable"/> and if <see cref="NullablityTypeKindExtension.HasTypeArguments()"/>
        /// is true, then all its subordinated types that are reference types are also nullables.
        /// <para>
        /// This flag can also be set simultaneously with the <see cref="NRTFullNonNullable"/>: when both are set it means that
        /// the type is marked with a complex NRT NullableAttribute.
        /// Use <see cref="NullablityTypeKindExtension.IsNRTFullNullable()"/> to test if this type is really NRT nullable.
        /// </para>
        /// </summary>
        NRTFullNullable = 64,

        /// <summary>
        /// Optional flag thar describes a Nullable Reference Type marked with NullableAttribute(1): the type
        /// is necessarily <see cref="IsReferenceType"/> and only <see cref="IsTechnicallyNullable"/> and
        /// if <see cref="NullablityTypeKindExtension.HasTypeArguments()"/> is true, then all its subordinated types that are reference types
        /// are also non nullables reference types.
        /// <para>
        /// This flag can also be set simultaneously with the <see cref="NRTFullNullable"/>: when both are set it means that
        /// the type is marked with a complex NRT NullableAttribute.
        /// Use <see cref="NullablityTypeKindExtension.IsNRTFullNonNullable()"/> to test if this type is really NRT nullable.
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

    /// <summary>
    /// Extends <see cref="NullabilityTypeKind"/>.
    /// </summary>
    public static class NullablityTypeKindExtension
    {
        /// <summary>
        /// Gets whether this has the <see cref="NullabilityTypeKind.IsReferenceType"/> flag.
        /// </summary>
        /// <param name="this">This <see cref="NullabilityTypeKind"/>.</param>
        /// <returns>True for reference types.</returns>
        public static bool IsReferenceType( this NullabilityTypeKind @this ) => (@this & NullabilityTypeKind.IsReferenceType) != 0;

        /// <summary>
        /// Gets whether this has the <see cref="NullabilityTypeKind.IsTupleType"/> flag.
        /// </summary>
        /// <param name="this">This <see cref="NullabilityTypeKind"/>.</param>
        /// <returns>True for tuple types.</returns>
        public static bool IsTupleType( this NullabilityTypeKind @this ) => (@this & NullabilityTypeKind.IsTupleType) != 0;

        /// <summary>
        /// Gets whether this is a non generic value type.
        /// Byte annotation is skipped.
        /// </summary>
        /// <param name="this">This <see cref="NullabilityTypeKind"/>.</param>
        /// <returns>True for non generic value types.</returns>
        public static bool IsNonGenericValueType( this NullabilityTypeKind @this ) => (@this & NullabilityTypeKind.IsValueType) != 0 && (@this & NullabilityTypeKind.IsGenericType) == 0;

        /// <summary>
        /// Gets whether this is a reference type that is used in a nullable aware context.
        /// </summary>
        /// <param name="this">This <see cref="NullabilityTypeKind"/>.</param>
        /// <returns>True for Nullable Reference Type aware types.</returns>
        public static bool IsNRTAware( this NullabilityTypeKind @this ) => (@this & (NullabilityTypeKind.NRTFullNonNullable | NullabilityTypeKind.NRTFullNullable)) != 0;

        /// <summary>
        /// Gets whether this is a NRT that is fully nullable.
        /// See <see cref="NullabilityTypeKind.NRTFullNullable"/>.
        /// </summary>
        /// <param name="this">This <see cref="NullabilityTypeKind"/>.</param>
        /// <returns>True for Nullable Reference Type fully null.</returns>
        public static bool IsNRTFullNullable( this NullabilityTypeKind @this ) => (@this & (NullabilityTypeKind.NRTFullNonNullable | NullabilityTypeKind.NRTFullNullable)) == NullabilityTypeKind.NRTFullNullable;

        /// <summary>
        /// Gets whether this is a NRT that is fully non nullable.
        /// See <see cref="NullabilityTypeKind.NRTFullNonNullable"/>.
        /// </summary>
        /// <param name="this">This <see cref="NullabilityTypeKind"/>.</param>
        /// <returns>True for Nullable Reference Type fully null.</returns>
        public static bool IsNRTFullNonNullable( this NullabilityTypeKind @this ) => (@this & (NullabilityTypeKind.NRTFullNonNullable | NullabilityTypeKind.NRTFullNullable)) == NullabilityTypeKind.NRTFullNonNullable;

        /// <summary>
        /// Gets whether this is a nullable type.
        /// See <see cref="NullabilityTypeKind.IsNullable"/>.
        /// </summary>
        /// <param name="this">This <see cref="NullabilityTypeKind"/>.</param>
        /// <returns>True for nullable type.</returns>
        public static bool IsNullable( this NullabilityTypeKind @this ) => (@this & NullabilityTypeKind.IsNullable) != 0;

        /// <summary>
        /// Gets whether this is a nullable value type.
        /// Byte annotation is skipped and the inner type must be lifted: <see cref="NullabilityTypeKind.IsGenericType"/> and <see cref="NullabilityTypeKind.IsTupleType"/>
        /// apply to the inner type.
        /// </summary>
        /// <param name="this">This <see cref="NullabilityTypeKind"/>.</param>
        /// <returns>True for nullable value type.</returns>
        public static bool IsNullableValueType( this NullabilityTypeKind @this ) => (@this & (NullabilityTypeKind.IsNullable|NullabilityTypeKind.IsValueType)) == (NullabilityTypeKind.IsNullable | NullabilityTypeKind.IsValueType);

        /// <summary>
        /// Gets whether this is a technically nullable type.
        /// See <see cref="NullabilityTypeKind.IsTechnicallyNullable"/>.
        /// </summary>
        /// <param name="this">This <see cref="NullabilityTypeKind"/>.</param>
        /// <returns>True for type that can be null (even if they shouldn't).</returns>
        public static bool IsTechnicallyNullable( this NullabilityTypeKind @this ) => (@this & NullabilityTypeKind.IsTechnicallyNullable) != 0;

        /// <summary>
        /// Gets a readeable string.
        /// </summary>
        /// <param name="this">This info.</param>
        /// <returns>A readable string.</returns>
        public static string ToStringClear( this NullabilityTypeKind @this )
        {
            var s = (@this & ~(NullabilityTypeKind.NRTFullNonNullable | NullabilityTypeKind.NRTFullNullable)).ToString();
            if( @this.IsNRTAware() )
            {
                s += " (NRT";
                if( @this.IsNRTFullNonNullable() ) s += ":FullNonNull)";
                else if( @this.IsNRTFullNullable() ) s += ":FullNull)";
                else s += ":Profile)";
            }
            return s;
        }
    }
}
