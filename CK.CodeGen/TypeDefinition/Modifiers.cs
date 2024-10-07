using System;

namespace CK.CodeGen;

/// <summary>
/// Simple flags that defines almost all "modifiers" that may be applied to
/// type, methods or other language items.
/// For function parameters, see <see cref="FunctionDefinition.ParameterModifier"/>.
/// </summary>
[Flags]
public enum Modifiers
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    None = 0,
    Readonly = 1,
    Static = 1 << 1,
    Internal = 1 << 2,
    Private = 1 << 3,
    Protected = 1 << 4,
    Public = 1 << 5,
    Abstract = 1 << 6,
    New = 1 << 7,
    Virtual = 1 << 8,
    Sealed = 1 << 9,
    Override = 1 << 10,
    Explicit = 1 << 11,
    Extern = 1 << 12,
    Implicit = 1 << 13,
    Unsafe = 1 << 14,
    Volatile = 1 << 15,
    Async = 1 << 16,
    Ref = 1 << 17,
    Partial = 1 << 18,
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    /// <summary>
    /// Applies only to class to define C# 9 data record.
    /// </summary>
    Data = 1 << 19
}
