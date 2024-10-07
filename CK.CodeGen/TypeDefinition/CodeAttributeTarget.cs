using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen;

/// <summary>
/// Describes the target of the <see cref="Attribute"/>.
/// See https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/attributes/#attribute-targets
/// </summary>
public enum CodeAttributeTarget
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    None,
    Assembly,
    Module,
    Field,
    Event,
    Method,
    Param,
    Property,
    Return,
    Type
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
