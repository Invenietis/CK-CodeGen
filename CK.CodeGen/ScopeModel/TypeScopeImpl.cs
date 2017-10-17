using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CK.CodeGen.Abstractions;

namespace CK.CodeGen
{
    sealed class TypeScopeImpl : CodeScopeImpl, ITypeScope
    {
        readonly static string HeaderTypeError = @"Unable to extract kind and type name from: '{0}'.";
        readonly static string[] _typeKind = new[] { "class", "interface", "enum", "struct" };
        readonly static Regex _nameStopper = new Regex( @"\s*(\bwhere\s|:|{)", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture );
        readonly static Regex _variantOutIn = new Regex( @"(?<!out|in)\s", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture );

        string _declaration;

        internal TypeScopeImpl( CodeWorkspace ws, ICodeScope parent )
            : base( ws, parent )
        {
            ICodeScope p = parent;
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

        /// <summary>
        /// Extracts the name (ignoring front modifiers, type name, generic parameters,
        /// base class, interfaces and generic constraints).
        /// The declaration itself is updated as one string and the scope opener is injected if needed.
        /// </summary>
        internal void Initialize()
        {
            var b = new StringBuilder();
            string decl = _declaration = Build( b, false ).ToString();
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
                    if( m.Success && m.Index > idx )
                    {

                        int endStopIdx = m.Index + m.Length;
                        bool hasOpenBrace = decl[endStopIdx - 1] == '{'
                                            || decl.IndexOf( '{', endStopIdx ) > 0;
                        if( !hasOpenBrace ) Code.Add( "{" );

                        string rawType = decl.Substring( idx, m.Index - idx );
                        SetName( ReferenceEquals( kind, "interface" )
                                    ? _variantOutIn.Replace( rawType, String.Empty )
                                    : rawType );
                    }
                }
            }
            throw new InvalidOperationException( string.Format( HeaderTypeError, decl ) );
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
            BuildCode( b );
            BuildTypes( b );
            if( closeScope ) b.AppendLine( "}" );
            return b;
        }
    }
}
