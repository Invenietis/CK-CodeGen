using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen.Abstractions
{
    /// <summary>
    /// Describes the target of the <see cref="Attribute"/>.
    /// See https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/attributes/#attribute-targets
    /// </summary>
    public enum CodeAttributeTarget
    {
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
    }
}
