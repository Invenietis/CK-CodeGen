namespace CK.CodeGen;

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
    public static bool IsNonGenericValueType( this NullabilityTypeKind @this ) => (@this & (NullabilityTypeKind.IsValueType | NullabilityTypeKind.IsGenericType)) == NullabilityTypeKind.IsValueType;

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
    /// This simply check the <see cref="NullabilityTypeKind.IsNullable"/> bit.
    /// </summary>
    /// <param name="this">This <see cref="NullabilityTypeKind"/>.</param>
    /// <returns>True for nullable type.</returns>
    public static bool IsNullable( this NullabilityTypeKind @this ) => (@this & NullabilityTypeKind.IsNullable) != 0;

    /// <summary>
    /// Gets whether this is a value type.
    /// This simply check the <see cref="NullabilityTypeKind.IsValueType"/> bit.
    /// </summary>
    /// <param name="this">This <see cref="NullabilityTypeKind"/>.</param>
    /// <returns>True for value type.</returns>
    public static bool IsValueType( this NullabilityTypeKind @this ) => (@this & NullabilityTypeKind.IsValueType) != 0;

    /// <summary>
    /// Gets whether this is a nullable value type.
    /// Byte annotation is skipped and the inner type must be lifted: <see cref="NullabilityTypeKind.IsGenericType"/> and <see cref="NullabilityTypeKind.IsTupleType"/>
    /// apply to the inner type.
    /// </summary>
    /// <param name="this">This <see cref="NullabilityTypeKind"/>.</param>
    /// <returns>True for nullable value type.</returns>
    public static bool IsNullableValueType( this NullabilityTypeKind @this ) => (@this & (NullabilityTypeKind.IsNullable | NullabilityTypeKind.IsValueType)) == (NullabilityTypeKind.IsNullable | NullabilityTypeKind.IsValueType);

    /// <summary>
    /// Gets whether this is a technically nullable type.
    /// Challenging null on a variable of this type should be done either because it is a nullable value type
    /// or a reference type (be it NRT nullable or not).
    /// </summary>
    /// <param name="this">This <see cref="NullabilityTypeKind"/>.</param>
    /// <returns>True for type that can be null (even if they shouldn't).</returns>
    public static bool IsTechnicallyNullable( this NullabilityTypeKind @this ) => (@this & (NullabilityTypeKind.IsNullable | NullabilityTypeKind.IsReferenceType)) != 0;

    /// <summary>
    /// Gets a readable string.
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
