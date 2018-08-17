using CK.CodeGen.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen
{
    class CodePart : ICodeWriter, INamedScope
    {
        readonly INamedScope _owner;
        internal readonly List<object> Code;

        public CodePart( INamedScope owner )
        {
            _owner = owner;
            Code = new List<object>();
        }

        public INamedScope PartOwner => _owner;

        public void DoAdd( string code )
        {
            if( !String.IsNullOrEmpty( code ) ) Code.Add( code );
        }

        internal SmarterStringBuilder Build( SmarterStringBuilder b )
        {
            b.AppendLine();
            foreach( var c in Code )
            {
                if( c is CodePart p ) p.Build( b );
                else b.Append( (string)c );
            }
            b.AppendLine();
            return b;
        }

        public StringBuilder BuildPart( StringBuilder b ) => Build( new SmarterStringBuilder() ).Builder;

        ICodeWorkspace INamedScope.Workspace => _owner.Workspace;

        INamedScope INamedScope.Parent => _owner.Parent;

        string INamedScope.Name => _owner.Name;

        string INamedScope.FullName => _owner.FullName;

        StringBuilder INamedScope.Build( StringBuilder b, bool closeScope ) => _owner.Build( b, closeScope );

        internal void MergeWith( CodePart other )
        {
            foreach( var c in other.Code )
            {
                if( c is CodePart p ) MergeWith( p );
                else Code.Add( (string)c );
            }
        }

        public override string ToString() => BuildPart( new StringBuilder() ).ToString();
    }
}
