using CK.CodeGen.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen
{
    struct FunctionDefiner
    {
        readonly Dictionary<string, FunctionScopeImpl> _funcs;

        public FunctionDefiner( bool dummy )
        {
            _funcs = new Dictionary<string, FunctionScopeImpl>();
        }

        public FunctionScopeImpl Create( CodeWorkspaceImpl ws, IFunctionDefinerScope h, Action<IFunctionScope> header )
        {
            if( header == null ) throw new ArgumentNullException( nameof( header ) );
            FunctionScopeImpl f = new FunctionScopeImpl( ws, h );
            header( f );
            f.Initialize();
            _funcs.Add( f.Name, f );
            return f;
        }

        public SmarterStringBuilder Build( SmarterStringBuilder b )
        {
            foreach( var t in _funcs.Values ) t.Build( b, true );
            return b;
        }

        public void MergeWith( CodeWorkspaceImpl ws, IFunctionDefinerScope h, FunctionDefiner other )
        {
            foreach( var kv in other._funcs )
            {
                if( !_funcs.TryGetValue( kv.Key, out var my ) )
                {
                    my = new FunctionScopeImpl( ws, h );
                    _funcs.Add( kv.Key, my );
                }
                my.MergeWith( kv.Value );
            }
        }
    }
}
