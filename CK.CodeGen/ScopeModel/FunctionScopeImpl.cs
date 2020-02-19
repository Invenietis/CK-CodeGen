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
    sealed class FunctionScopeImpl : NamedScopeImpl, IFunctionScope
    {
        readonly FunctionDefiner _funcs;

        MethodDefinition _mDef;
        ExposedName _name;
        string _declaration;
        int _codeStartIdx;

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

        public bool IsConstructor => _mDef?.ReturnType == null;

        public string ReturnType => _mDef?.ReturnType?.ToString();

        public IFunctionName FunctionName => _name;

        internal void MergeWith( FunctionScopeImpl other )
        {
            Debug.Assert( other != null );
            if( other._codeStartIdx > 0 )
            {
                CodePart.Parts.Add( other._declaration.Substring( _codeStartIdx ) );
            }
            CodePart.MergeWith( other.CodePart );
            _funcs.MergeWith( Workspace, this, other._funcs );
        }

        class ExposedName : IFunctionName
        {
            readonly string _text;

            public ExposedName( MethodDefinition m )
            {
                NakedName = m.MethodName.Name;
                var b = new StringBuilder();
                GenericPart = m.MethodName.WriteGenArgs( b ).ToString();
                b.Clear();
                ParametersPart = m.WriteParameters( b, withAttributes: false, withDefaultValues: false ).ToString();
                _text = NakedName + GenericPart + ParametersPart;
            }

            public string NakedName { get; }

            public string GenericPart { get; }

            public string ParametersPart { get; }

            public override string ToString() => _text;
        }

        internal void Initialize()
        {
            var b = new SmarterStringBuilder( new StringBuilder() );
            // We store the declaration and clears the code buffer.
            _declaration = CodePart.Build( b ).ToString();
            CodePart.Parts.Clear();
            var m = new StringMatcher( _declaration );
            m.SkipWhiteSpacesAndJSComments();
            if( !m.MatchMethodDefinition( out _mDef, out bool hasCodeOpener ) )
            {
                throw new InvalidOperationException( $"Error: {m.ErrorMessage} Unable to parse function or constructor declaration {_declaration}" );
            }
            if( hasCodeOpener )
            {
                m.MatchWhiteSpaces( 0 );
                _codeStartIdx = m.StartIndex;
            }
            _name = new ExposedName( _mDef );
            SetName( _name.ToString() );
        }

        internal protected override SmarterStringBuilder Build( SmarterStringBuilder b, bool closeScope )
        {
            if( _declaration == null ) CodePart.Build( b );
            else
            {
                b.AppendLine().Append( _declaration );
                if( _codeStartIdx == 0 ) b.AppendLine().Append( "{" ).AppendLine();
                CodePart.Build( b );
                _funcs.Build( b );
                if( closeScope ) b.AppendLine().Append( "}" );
            }
            return b;
        }

        public IFunctionScope CreateFunction( Action<IFunctionScope> header )
        {
            return _funcs.Create( Workspace, this, header );
        }

        public IFunctionScopePart CreatePart( bool top )
        {
            var p = new Part( this );
            if( top ) CodePart.Parts.Insert( 0, p );
            else CodePart.Parts.Add( p );
            return p;
        }

        class Part : CodePart, IFunctionScopePart
        {
            public Part( IFunctionScope owner )
                : base( owner )
            {
            }

            public new IFunctionScope PartOwner => (IFunctionScope)base.PartOwner;

            public IFunctionName FunctionName => PartOwner.FunctionName;

            public ITypeScope EnclosingType => PartOwner.EnclosingType;

            public bool IsLocalFunction => PartOwner.IsLocalFunction;

            public bool IsConstructor => PartOwner.IsConstructor;

            public string ReturnType => PartOwner.ReturnType;

            public IFunctionScope CreateFunction( Action<IFunctionScope> header ) => PartOwner.CreateFunction( header );

            public IFunctionScopePart CreatePart( bool top ) 
            {
                var p = new Part( this );
                if( top ) Parts.Insert( 0, p );
                else Parts.Add( p );
                return p;
            }

        }

    }
}
