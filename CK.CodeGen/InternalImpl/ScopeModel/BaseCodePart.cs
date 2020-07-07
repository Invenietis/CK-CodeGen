using CK.CodeGen;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen
{
    class BaseCodePart : ICodeWriter
    {
        internal readonly List<object> Parts;
        Dictionary<object, object?>? _memory;

        public BaseCodePart()
        {
            Parts = new List<object>();
        }

        public void DoAdd( string code )
        {
            if( !String.IsNullOrEmpty( code ) ) Parts.Add( code );
        }

        internal SmarterStringBuilder Build( SmarterStringBuilder b )
        {
            b.AppendLine();
            foreach( var c in Parts )
            {
                if( c is BaseCodePart p ) p.Build( b );
                else b.Append( (string)c );
            }
            b.AppendLine();
            return b;
        }

        public void BuildPart( Action<string> collector ) => Build( new SmarterStringBuilder( collector ) );

        public StringBuilder Build( StringBuilder b, bool closeScope ) => Build( new SmarterStringBuilder( b ) ).Builder!;

        internal void MergeWith( BaseCodePart other )
        {
            foreach( var c in other.Parts )
            {
                if( c is BaseCodePart p ) MergeWith( p );
                else Parts.Add( (string)c );
            }
        }

        public IDictionary<object, object?> Memory => _memory ?? (_memory = new Dictionary<object, object?>());

        public override string ToString() => Build( new SmarterStringBuilder( new StringBuilder() ) ).ToString();
    }
}
