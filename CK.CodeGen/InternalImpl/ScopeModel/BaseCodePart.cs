using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen;

class BaseCodePart : ICodeWriter
{
    internal readonly List<object> Parts;
    Dictionary<object, object?>? _memory;

    public BaseCodePart()
    {
        Parts = new List<object>();
    }

    public void DoAdd( string? code )
    {
        if( !String.IsNullOrEmpty( code ) ) Parts.Add( code );
    }

    internal bool? StartsWith( string prefix )
    {
        foreach( var o in Parts )
        {
            if( o is string s )
            {
                s = s.TrimStart();
                if( s.Length > 0 ) return s.StartsWith( prefix, StringComparison.Ordinal );
            }
            else
            {
                bool? r = ((BaseCodePart)o).StartsWith( prefix );
                if( r.HasValue ) return r;
            }
        }
        return null;
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

    public StringBuilder Build( StringBuilder b, bool closeScope ) => Build( new SmarterStringBuilder( b ) ).Builder!;

    internal void MergeWith( BaseCodePart other )
    {
        foreach( var c in other.Parts )
        {
            if( c is BaseCodePart p ) MergeWith( p );
            else Parts.Add( (string)c );
        }
    }

    public IDictionary<object, object?> Memory => _memory ??= new Dictionary<object, object?>();

    public override string ToString() => Build( new SmarterStringBuilder( new StringBuilder() ) ).ToString();
}
