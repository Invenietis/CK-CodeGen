using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen.Abstractions
{
    /// <summary>
    /// A simple <see cref="ICodeWriter"/> wrapped around a <see cref="StringBuilder"/>.
    /// </summary>
    public class StringCodeWriter : ICodeWriter
    {
        /// <summary>
        /// Initializes a new <see cref="StringCodeWriter"/>.
        /// </summary>
        /// <param name="b">Optional existing StringBuilder. A new one is created by default.</param>
        public StringCodeWriter( StringBuilder b = null )
        {
            StringBuilder = b ?? new StringBuilder();
        }

        /// <summary>
        /// Gets the current output.
        /// </summary>
        public StringBuilder StringBuilder { get; }

        void ICodeWriter.DoAdd( string code ) => StringBuilder.Append( code );

        /// <summary>
        /// Overridden to return the <see cref="StringBuilder"/> content.
        /// </summary>
        /// <returns>The current code.</returns>
        public override string ToString() => StringBuilder.ToString();
    }
}
