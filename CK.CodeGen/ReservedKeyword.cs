using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen
{
    /// <summary>
    /// Exposes the C# <see cref="ReservedKeywords"/>.
    /// </summary>
    public static class ReservedKeyword
    {
        static readonly string[] _reserved = new[]{ "abstract", "as", "base", "bool",
                                                     "break", "byte", "case", "catch",
                                                     "char", "checked", "class   const",
                                                     "continue", "decimal", "default", "delegate",
                                                     "do", "double", "else", "enum",
                                                     "event", "explicit", "extern", "false",
                                                     "finally", "fixed", "float", "for",
                                                     "foreach", "goto", "if", "implicit",
                                                     "in", "int", "interface", "internal",
                                                     "is", "lock", "long", "namespace",
                                                     "new", "null", "object", "operator",
                                                     "out", "override", "params", "private",
                                                     "protected", "public", "readonly", "record",
                                                     "ref", "return", "sbyte", "sealed",
                                                     "short", "sizeof", "stackalloc", "static",
                                                     "string", "struct", "switch", "this",
                                                     "throw", "true", "try", "typeof",
                                                     "uint", "ulong", "unchecked", "unsafe",
                                                     "ushort", "using", "virtual", "void",
                                                     "volatile", "while" };

        /// <summary>
        /// Gets the sorted array of reserved keywords.
        /// From https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/.
        /// </summary>
        public static ReadOnlySpan<string> ReservedKeywords => _reserved.AsSpan();

        /// <summary>
        /// Tests whether a variable name or mere string is a C# reserved keyword.
        /// </summary>
        /// <param name="v">The string.</param>
        /// <returns>True if it's a reserved keyword.</returns>
        public static bool IsReservedKeyword( string v ) => _reserved.AsSpan().BinarySearch( v ) >= 0;

    }
}
