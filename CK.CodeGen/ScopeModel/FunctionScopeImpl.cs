using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CK.CodeGen.Abstractions;

namespace CK.CodeGen
{
    sealed class FunctionScopeImpl : NamedScopeImpl, IFunctionScope
    {
        readonly static string HeaderTypeError = @"Unable to extract function name from: '{0}'.";
        readonly static Regex _frontModifiers = new Regex( "^\\s*((public|protected|internal|private|protected|internal|new|abstract|virtual|override|sealed|static|readonly|extern|unsafe|volatile|async)\\s+)*", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture );
        readonly static Regex _quickAndDirtyType = new Regex( @"^\w(\w|\.)*\s*(<(?:[^<>]|(?<1><)|(?<-1>>))+(?(1)(?!))>)?(\[(\s|\,)*])*\s*", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture );
        readonly static Regex _recursePar = new Regex( @"^\(((?:[^()]|(?<1>\()|(?<-1>\)))+(?(1)(?!)))?\)", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture );
        string _declaration;
        int _codeStartIdx;
        string _returnType;
        FunctionDefiner _funcs;

        internal FunctionScopeImpl( CodeWorkspaceImpl ws, INamedScope parent )
            : base( ws, parent )
        {
            _funcs = new FunctionDefiner( true );
            INamedScope p = parent;
            for(; ; )
            {
                if( p is ITypeScope t )
                {
                    EnclosingType = t;
                    break;
                }
                p = p.Parent;
                Debug.Assert( p != null, "We eventually reached a top level type." );
            }
        }

        public ITypeScope EnclosingType { get; }

        public bool IsLocalFunction => Parent is IFunctionScope;

        public bool IsConstructor => _returnType == null;

        public string ReturnType => _returnType;

        internal void MergeWith( FunctionScopeImpl other )
        {
            Debug.Assert( other != null );
            if( other._codeStartIdx > 0 )
            {
                Code.Add( other._declaration.Substring( _codeStartIdx ) );
            }
            _funcs.MergeWith( Workspace, this, other._funcs );
            base.MergeWith( this );
        }

        internal void Initialize()
        {
            var b = new StringBuilder();
            // We store the declaration and clears the code buffer.
            string decl = _declaration = BuildCode( b ).ToString();
            Code.Clear();
            Debug.Assert( _frontModifiers.Match( decl ).Success );
            Match mFront = _frontModifiers.Match( decl );
            int idxStart = mFront.Index + mFront.Length;
            // Caution!
            // m.Match( decl.Substring( idx ) ) !== m.Match( decl, idx ) )
            // Because of ^
            Match mId1 = _quickAndDirtyType.Match( decl.Substring( idxStart ) );
            if( mId1.Success )
            {
                string rawName;
                _returnType = rawName = RemoveWhiteSpaces( mId1.Value );
                int idxNext = idxStart + mId1.Index + mId1.Length;
                Match mId2 = _quickAndDirtyType.Match( decl.Substring( idxNext ) );
                if( mId2.Success )
                {
                    rawName = RemoveWhiteSpaces( mId2.Value );
                    idxNext += mId2.Index + mId2.Length;
                }
                Match args = _recursePar.Match( decl.Substring( idxNext ) );
                if( args.Success )
                {
                    if( ReferenceEquals( _returnType, rawName ) )
                    {
                        // ctor
                        _returnType = null;
                    }
                    SetName( rawName + RemoveWhiteSpaces( args.Value ) );
                    idxNext += args.Index + args.Length;
                    _codeStartIdx = decl.IndexOf( '{', idxNext ) + 1;
                    return;
                }
            }
            throw new InvalidOperationException( string.Format( HeaderTypeError, decl ) );
        }

        public override StringBuilder Build( StringBuilder b, bool closeScope )
        {
            b.Append( _declaration );
            if( _codeStartIdx == 0 ) b.Append( Environment.NewLine ).Append( '{' ).Append( Environment.NewLine );
            BuildCode( b );
            _funcs.Build( b );
            if( closeScope ) b.AppendLine( "}" );
            return b;
        }

        public IFunctionScope CreateFunction( Action<IFunctionScope> header )
        {
            return _funcs.Create( Workspace, this, header );
        }
    }
}
