using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen
{
    internal static class TypeExtensions
    {
        internal static string GetSourceName(this Type @this) => @this.IsGenericParameter ? @this.Name : @this.FullName;
    }
}
