using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen
{
    class SmarterStringBuilder
    {
        public readonly StringBuilder Builder;
        bool _hasNewLine;

        public SmarterStringBuilder( StringBuilder b = null )
        {
            Builder = b ?? new StringBuilder();
            _hasNewLine = true;
        }

        public SmarterStringBuilder Append( string s )
        {
            Builder.Append( s );
            _hasNewLine = s.EndsWith( Environment.NewLine, StringComparison.Ordinal );
            return this;
        }

        public SmarterStringBuilder AppendLine()
        {
            if( !_hasNewLine )
            {
                Builder.AppendLine();
                _hasNewLine = true;
            }
            return this;
        }

        public SmarterStringBuilder Append( char c )
        {
            Builder.Append( c );
            _hasNewLine = false;
            return this;
        }

        public override string ToString() => Builder.ToString();
    }
}
