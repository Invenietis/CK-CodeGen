using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Globalization;
using System.Diagnostics;
using CK.CodeGen;

namespace CK.CodeGen
{
    /// <summary>
    /// Adds extension methods related to source code generation to basic types.
    /// </summary>
    public static class ExternalTypeExtensions
    {
        /// <summary>
        /// Gets whether this type is a <see cref="ValueTuple"/>.
        /// </summary>
        /// <param name="this"></param>
        /// <returns>True if this is a value tuple. False otherwise.</returns>
        public static bool IsValueTuple( this Type? @this )
        {
            return @this != null && @this.Namespace == "System" && @this.Name.StartsWith( "ValueTuple`", StringComparison.Ordinal );
        }

        /// <summary>
        /// Gets the code required to dynamically obtain the type. It is either "null", "typeof(void)"
        /// or the call to "Type.GetType(...)" with the assembly qualified name of this type.
        /// </summary>
        /// <param name="this">This type. Can be null.</param>
        /// <returns>The code to obtain this Type' type.</returns>
        static public string ToGetTypeSourceString( this Type? @this )
        {
            return @this == null
                    ? "null"
                    : (@this != typeof( void )
                        ? "Type.GetType(" + @this.AssemblyQualifiedName.ToSourceString() + ')'
                        : "typeof(void)");
        }

        /// <summary>
        /// Obtains the code that represents this string. It is either "null" or
        /// a verbatim string in which " are correctly doubled.
        /// </summary>
        /// <param name="this">This string. Can be null.</param>
        /// <returns>The code to represent it.</returns>
        static public string ToSourceString( this string? @this )
        {
            return @this == null
                        ? "null"
                        : $"@\"{@this.Replace( "\"", "\"\"", StringComparison.Ordinal )}\"";
        }
    }
}
