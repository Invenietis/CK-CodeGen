using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen
{
    public static class TypeExtensions
    {
        public static string GetSourceName(this Type @this) => @this.IsGenericParameter ? @this.Name : @this.FullName.Replace("+",".");
    }
}
