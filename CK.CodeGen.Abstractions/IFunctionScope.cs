using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen.Abstractions
{
    /// <summary>
    /// A function is defined in a <see cref="ITypeScope"/> or in another <see cref="IFunctionScope"/>.
    /// </summary>
    public interface IFunctionScope : IFunctionDefinerScope
    {
        /// <summary>
        /// Gets the function name description.
        /// </summary>
        IFunctionName FunctionName { get; }

        /// <summary>
        /// Gets the closest type that contains this function or method.
        /// </summary>
        ITypeScope EnclosingType { get; }

        /// <summary>
        /// Gets whether this function is a local function.
        /// </summary>
        bool IsLocalFunction { get; }

        /// <summary>
        /// Gets whether this function is a constructor (its <see cref="ReturnType"/> is null).
        /// </summary>
        bool IsConstructor { get; }

        /// <summary>
        /// Gets the return type of the function.
        /// Null when <see cref="IsConstructor"/> is true.
        /// </summary>
        string ReturnType { get; }
    }
}
