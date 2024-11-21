using System;

namespace CK.CodeGen;

/// <summary>
/// Hook into <see cref="NullableTypeTree"/> creation.
/// </summary>
public interface INullableTypeTreeBuilder
{
    /// <summary>
    /// Must create a <see cref="NullableTypeTree"/> from its components.
    /// </summary>
    /// <param name="t">The type.</param>
    /// <param name="kind">The computed nullability.</param>
    /// <param name="subTypes">The created subordinated types.</param>
    /// <param name="genericArguments">The <see cref="Type.GetGenericArguments()"/> if the type is a generic.</param>
    /// <returns></returns>
    NullableTypeTree Create( Type t, NullabilityTypeKind kind, NullableTypeTree[] subTypes, Type[]? genericArguments = null );
}

