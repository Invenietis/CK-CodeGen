using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen.Abstractions.Tests
{
    public class SimpleCodeWriter : ICodeWriter
    {
        readonly List<string> _code = new List<string>();

        public void DoAdd( string code )
        {
            if( !String.IsNullOrEmpty( code ) ) _code.Add( code );
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach( string s in _code ) sb.Append( s );
            var r = sb.ToString();
            _code.Clear();
            _code.Add( r );
            return r;
        }
    }
}
