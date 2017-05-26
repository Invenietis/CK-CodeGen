using System;
using System.Collections.Generic;
using System.Text;
using CK.Text;
using System.Linq;

namespace CK.CodeGen
{
    public static class CodeGenExtensions
    {
        static public string ToSourceString(this string @this)
        {
            return @this == null ? "null" : $"@\"{@this.Replace("\"", "\"\"")}\"";
        }

        static public StringBuilder ToSourceString(this byte[] @this, StringBuilder b)
        {
            if (@this == null) return b.Append("null");
            if (@this.Length == 0) return b.Append("Array.Empty<byte>()");
            return b.Append("new byte[] {").AppendStrings(@this.Select(x => x.ToString()),",").Append('}');
        }

        static public StringBuilder AppendSourceString(this StringBuilder @this, byte[] b) => ToSourceString(b, @this);

        static public StringBuilder AppendSourceString(this StringBuilder @this, string s) => @this.Append(ToSourceString(s));

    }


}
