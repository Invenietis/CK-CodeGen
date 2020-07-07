using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen
{
    /// <summary>
    /// Internal class that handles new lines and unifies the output to a StringBuilder
    /// or an Action{string}.
    /// </summary>
    class SmarterStringBuilder
    {
        readonly Action<string> _collector;

        /// <summary>
        /// Exposes the wrapped String builder or null if this
        /// builder is based on a Action{string} delegate.
        /// </summary>
        public readonly StringBuilder? Builder;

        public bool HasNewLine { get; set; }

        public SmarterStringBuilder( StringBuilder b )
        {
            Builder = b;
            _collector = s => b.Append( s );
            HasNewLine = true;
        }

        public SmarterStringBuilder( Action<string> collector )
        {
            _collector = collector;
            HasNewLine = true;
        }

        public SmarterStringBuilder Append( string s )
        {
            _collector( s );
            HasNewLine = s.EndsWith( Environment.NewLine, StringComparison.Ordinal );
            return this;
        }

        public SmarterStringBuilder AppendLine()
        {
            if( !HasNewLine )
            {
                _collector( Environment.NewLine );
                HasNewLine = true;
            }
            return this;
        }

        public override string ToString() => Builder?.ToString() ?? "";
    }
}
