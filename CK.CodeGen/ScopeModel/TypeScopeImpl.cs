using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CK.CodeGen.Abstractions;

namespace CK.CodeGen
{
    sealed class TypeScopeImpl : TypeDefinerScopeImpl, ITypeScope
    {
        readonly static string HeaderTypeError = @"Unable to extract kind and type name from: '{0}'.";
        readonly static string[] _typeKind = new[] { "class", "interface", "enum", "struct" };
        readonly static Regex _nameStopper = new Regex( @"\s*(\bwhere\s+\p{L}|:|{)", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture );
        readonly FunctionDefiner _funcs;

        string _declaration;
        int _codeStartIdx;

        internal TypeScopeImpl( CodeWorkspaceImpl ws, INamedScope parent )
            : base( ws, parent )
        {
            _funcs = new FunctionDefiner( true );
            INamedScope p = parent;
            for( ; ;)
            {
                if( p is INamespaceScope ns )
                {
                    Namespace = ns;
                    break;
                }
                p = p.Parent;
                Debug.Assert( p != null, "We eventually reached the root namespace." );
            }
        }

        public INamespaceScope Namespace { get; }

        internal void MergeWith( TypeScopeImpl other )
        {
            Debug.Assert( other != null );
            if( other._codeStartIdx > 0 )
            {
                Code.Add( other._declaration.Substring( _codeStartIdx ) );
            }
            _funcs.MergeWith( Workspace, this, other._funcs );
            base.MergeWith( this );
        }

        /// <summary>
        /// Extracts the name.
        /// The declaration itself is updated as one string and the scope opener is injected if needed.
        /// </summary>
        internal void Initialize()
        {
            var b = new StringBuilder();
            // We store the declaration and clears the code buffer.
            string decl = _declaration = BuildCode( b ).ToString();
            Code.Clear();
            string kind;
            int idx = IndexOfKind( decl, out kind );
            if( idx >= 0 )
            {
                idx += kind.Length + 1;
                while( idx < decl.Length && Char.IsWhiteSpace( decl, idx ) ) ++idx;
                if( idx < decl.Length )
                {
                    Match m = _nameStopper.Match( decl, idx );
                    if( m.Success )
                    {
                        if( m.Index > idx )
                        {
                            int endStopIdx = m.Index + m.Length;
                            _codeStartIdx = decl[endStopIdx - 1] == '{'
                                                ? endStopIdx
                                                : decl.IndexOf( '{', endStopIdx ) + 1;
                            SetCleanTypeName( kind, decl.Substring( idx, m.Index - idx ) );
                            return;
                        }
                        // The stopper starts: there is no type name.
                    }
                    else
                    {
                        // No stopper found: the type name is from idx to the end.
                        var rawType = decl.Substring( idx ).TrimEnd();
                        if( rawType.Length > 0 )
                        {
                            SetCleanTypeName( kind, rawType );
                            return;
                        }
                    }
                }
            }
            throw new InvalidOperationException( string.Format( HeaderTypeError, decl ) );
        }

        void SetCleanTypeName( string kind, string rawType )
        {
            var typeName = ReferenceEquals( kind, "interface" )
                        ? RemoveVariantInOut( rawType )
                        : rawType;
            SetName( RemoveWhiteSpaces( typeName ) );
        }

        static int IndexOfKind( string s, out string found )
        {
            found = null;
            int bestIdx = Int32.MaxValue;
            foreach( string value in _typeKind )
            {
                int idx = s.IndexOf( value );
                if( idx >= 0
                    && idx < bestIdx
                    && (idx == 0 || Char.IsWhiteSpace(s,idx-1)) )
                {
                    int end = idx + value.Length;
                    if( end < s.Length && Char.IsWhiteSpace( s, end ) )
                    {
                        found = value;
                        bestIdx = idx;
                    }
                }
            }
            return found != null ? bestIdx : -1;
        }

        public override StringBuilder Build( StringBuilder b, bool closeScope )
        {
            b.Append( _declaration );
            if( _codeStartIdx == 0 ) b.Append( Environment.NewLine ).Append( '{' ).Append( Environment.NewLine );
            BuildCode( b );
            _funcs.Build( b );
            BuildTypes( b );
            if( closeScope ) b.AppendLine( "}" );
            return b;
        }

        public IFunctionScope CreateFunction( Action<IFunctionScope> header )
        {
            return _funcs.Create( Workspace, this, header );
        }
    }
}
