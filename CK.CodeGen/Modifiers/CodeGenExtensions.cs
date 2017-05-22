using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen
{
    public static class CodeGenExtensions
    {
        static public string ToSourceString(this string @this)
        {
            return $"@\"{@this.Replace("\"", "\"\"")}\"";
        }

    }


}
