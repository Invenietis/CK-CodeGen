using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen.Abstractions
{
    public interface IFunctionName
    {
        /// <summary>
        /// Gets the name only part without generic or formal parameters.
        /// </summary>
        string NakedName { get; }

        /// <summary>
        /// Gets the generic part if any (the empty string otherwise).
        /// </summary>
        string GenericPart { get; }

        /// <summary>
        /// Gets the parameters (including the enclosing brackets).
        /// </summary>
        string ParametersPart { get; }

        /// <summary>
        /// Returns the name of this function that contains the <see cref="NakedName"/>,
        /// <see cref="GenericPart"/> and <see cref="ParametersPart"/> in a normalized form.
        /// </summary>
        /// <returns>The function name.</returns>
        string ToString();
    }
}
