using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CK.CodeGen.Abstractions
{
    /// <summary>
    /// A function definer scope is <see cref="INamedScope"/> that can create <see cref="IFunctionScope"/>.
    /// It is itself a <see cref="ICodeWriter"/>: raw code can be appendend to it as needed.
    /// </summary>
    public interface IFunctionDefinerScope : INamedScope
    {
        /// <summary>
        /// Creates a <see cref="IFunctionScope"/> inside this scope.
        /// Its name is automatically extracted from the header that may contain the
        /// opening curly brace '{' or not (in such case it is automatically appended).
        /// </summary>
        /// <param name="header">Configure the header. Must not be null.</param>
        /// <returns>The new type scope.</returns>
        IFunctionScope CreateFunction( Action<IFunctionScope> header );

    }
}
