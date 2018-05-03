using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CK.CodeGen.Abstractions;
using CK.Text;

namespace CK.CodeGen
{
    sealed class TypeScopeImpl : TypeDefinerScopeImpl, ITypeScope
    {
        readonly FunctionDefiner _funcs;

        TypeDefinition _typeDef;
        string _declaration;
        int _codeStartIdx;

        internal TypeScopeImpl( CodeWorkspaceImpl ws, INamedScope parent )
            : base( ws, parent )
        {
            _funcs = new FunctionDefiner( true );
            INamedScope p = parent;
            for(; ; )
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

        public bool IsNestedType => Parent is ITypeScope;

        internal string TypeKey => _typeDef.Name.TypeKey;

        internal void MergeWith( TypeScopeImpl other )
        {
            Debug.Assert( other != null );
            if( !_typeDef.Equals( other._typeDef ) )
            {
                throw new InvalidOperationException( $"Unable to merge type '{_typeDef}' with '{other._typeDef}'." );
            }
            if( other._codeStartIdx > 0 )
            {
                Code.Add( other._declaration.Substring( _codeStartIdx ) );
            }
            MergeCode( other );
            MergeTypes( other );
            _funcs.MergeWith( Workspace, this, other._funcs );
        }

        /// <summary>
        /// Extracts the name.
        /// The declaration itself is updated as one string and the scope opener is injected if needed.
        /// </summary>
        internal void Initialize()
        {
            var b = new SmarterStringBuilder( null );
            // We store the declaration and clears the code buffer.
            _declaration = BuildCode( b ).ToString();
            Code.Clear();
            var m = new StringMatcher( _declaration );
            m.SkipWhiteSpacesAndJSComments();
            if( !m.MatchTypeDefinition( out _typeDef, IsNestedType, out bool hasCodeOpener ) )
            {
                throw new InvalidOperationException( $"Error: {m.ErrorMessage} Unable to parse type declaration {_declaration}" );
            }
            if( hasCodeOpener )
            {
                m.MatchWhiteSpaces( 0 );
                _codeStartIdx = m.StartIndex;
            }
            SetName( _typeDef.Name.ToString() );
        }

        internal protected override SmarterStringBuilder Build( SmarterStringBuilder b, bool closeScope )
        {
            b.AppendLine().Append( _declaration );
            if( _codeStartIdx == 0 ) b.AppendLine().Append( '{' ).AppendLine();
            BuildCode( b );
            _funcs.Build( b );
            BuildTypes( b );
            if( closeScope ) b.AppendLine().Append( '}' ).AppendLine();
            return b;
        }

        public IFunctionScope CreateFunction( Action<IFunctionScope> header )
        {
            return _funcs.Create( Workspace, this, header );
        }

        public override string ToString() => _typeDef.Write( new StringBuilder() ).ToString();
    }
}
