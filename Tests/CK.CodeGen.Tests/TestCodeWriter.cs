using System.Collections.Generic;
using System.Text;
using CK.CodeGen.Abstractions;

namespace CK.CodeGen.Tests
{
    class TestCodeWriter : ICodeWriter
    {
        readonly List<string> _code = new List<string>();

        public ICodeWriter RawAppend( string code )
        {
            _code.Add( code );
            return this;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach( string s in _code ) sb.Append( s );
            return sb.ToString();
        }
    }
}
