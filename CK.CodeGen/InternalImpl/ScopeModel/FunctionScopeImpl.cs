using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CK.CodeGen;
using CK.Text;

namespace CK.CodeGen
{
    sealed class FunctionScopeImpl : NamedScopeImpl, IFunctionScope
    {
        readonly FunctionDefiner _funcs;

        FunctionDefinition? _fDef;

        internal FunctionScopeImpl( CodeWorkspaceImpl ws, INamedScope parent )
            : base( ws, parent )
        {
            _funcs = new FunctionDefiner( true );
            INamedScope? p = parent;
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

        internal FunctionScopeImpl( CodeWorkspaceImpl ws, INamedScope parent, FunctionDefinition def )
            : this( ws, parent )
        {
            _fDef = def;
            SetName( _fDef.Key );
        }

        public ITypeScope EnclosingType { get; }

        public bool IsLocalFunction => Parent is IFunctionScope;

        public bool IsConstructor => _fDef?.ReturnType == null;

        public FunctionDefinition Definition => _fDef!;

        public IReadOnlyCollection<IFunctionScope> Functions => _funcs.Functions;

        internal void MergeWith( FunctionScopeImpl other )
        {
            Debug.Assert( other != null );
            CodePart.MergeWith( other.CodePart );
            _funcs.MergeWith( Workspace, this, other._funcs );
        }

        internal void Initialize()
        {
            var b = new SmarterStringBuilder( new StringBuilder() );
            // We store the declaration and clears the code buffer.
            var declaration = CodePart.Build( b ).ToString();
            CodePart.Parts.Clear();
            var m = new StringMatcher( declaration );
            m.SkipWhiteSpacesAndJSComments();
            if( !m.MatchMethodDefinition( out _fDef, out bool hasCodeOpener ) )
            {
                throw new InvalidOperationException( $"Error: {m.ErrorMessage} Unable to parse function or constructor declaration {declaration}" );
            }
            Debug.Assert( _fDef != null );
            if( hasCodeOpener )
            {
                m.MatchWhiteSpaces( 0 );
                CodePart.Parts.Add( declaration.Substring( m.StartIndex ) );
            }
            SetName( _fDef.Key );
        }

        internal protected override SmarterStringBuilder Build( SmarterStringBuilder b, bool closeScope )
        {
            if( _fDef != null )
            {
                if( b.Builder != null ) _fDef.Write( b.Builder );
                else b.Append( _fDef.ToString() );
                b.HasNewLine = false;
            }
            bool lambda = CodePart.StartsWith( "=>" ) == true;
            if( !lambda ) b.AppendLine().Append( "{" ).AppendLine();
            CodePart.Build( b );
            _funcs.Build( b );
            if( closeScope && !lambda ) b.AppendLine().Append( "}" );
            return b;
        }

        public IFunctionScope CreateFunction( Action<IFunctionScope> header )
        {
            return _funcs.Create( Workspace, this, header );
        }
        public IFunctionScope CreateFunction( FunctionDefinition def )
        {
            return _funcs.Create( Workspace, this, def );
        }

        public IFunctionScopePart CreatePart( bool top )
        {
            var p = new Part( this );
            if( top ) CodePart.Parts.Insert( 0, p );
            else CodePart.Parts.Add( p );
            return p;
        }

        public IFunctionScope? FindFunction( string key, bool analyzeHeader ) => _funcs.FindFunction( key, analyzeHeader );

        class Part : CodePart, IFunctionScopePart
        {
            public Part( IFunctionScope owner )
                : base( owner )
            {
            }

            public new IFunctionScope PartOwner => (IFunctionScope)base.PartOwner;

            public FunctionDefinition Definition => PartOwner.Definition;

            public ITypeScope EnclosingType => PartOwner.EnclosingType;

            public bool IsLocalFunction => PartOwner.IsLocalFunction;

            public bool IsConstructor => PartOwner.IsConstructor;

            public IReadOnlyCollection<IFunctionScope> Functions => PartOwner.Functions;

            public IFunctionScope CreateFunction( Action<IFunctionScope> header ) => PartOwner.CreateFunction( header );

            public IFunctionScope CreateFunction( FunctionDefinition def ) => PartOwner.CreateFunction( def );

            public IFunctionScopePart CreatePart( bool top ) 
            {
                var p = new Part( this );
                if( top ) Parts.Insert( 0, p );
                else Parts.Add( p );
                return p;
            }

            public IFunctionScope? FindFunction( string key, bool analyzeHeader ) => PartOwner.FindFunction( key, analyzeHeader );
        }

    }
}
