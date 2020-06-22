using CK.CodeGen.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen
{
    class CodePart : ICodeWriter, INamedScope
    {
        readonly INamedScope _owner;
        internal readonly List<object> Parts;
        Dictionary<object, object?>? _memory;

        public CodePart( INamedScope owner )
        {
            _owner = owner;
            Parts = new List<object>();
        }

        public INamedScope PartOwner => _owner;

        public void DoAdd( string code )
        {
            if( !String.IsNullOrEmpty( code ) ) Parts.Add( code );
        }

        internal SmarterStringBuilder Build( SmarterStringBuilder b )
        {
            b.AppendLine();
            foreach( var c in Parts )
            {
                if( c is CodePart p ) p.Build( b );
                else b.Append( (string)c );
            }
            b.AppendLine();
            return b;
        }

        public void BuildPart( Action<string> collector ) => Build( new SmarterStringBuilder( collector ) );

        public StringBuilder Build( StringBuilder b, bool closeScope ) => Build( new SmarterStringBuilder( b ) ).Builder!;

        ICodeWorkspace INamedScope.Workspace => _owner.Workspace;

        INamedScope? INamedScope.Parent => _owner.Parent;

        string INamedScope.Name => _owner.Name;

        string INamedScope.FullName => _owner.FullName;

        void INamedScope.Build( Action<string> collector, bool closeScope ) => _owner.Build( collector, closeScope );

        internal void MergeWith( CodePart other )
        {
            foreach( var c in other.Parts )
            {
                if( c is CodePart p ) MergeWith( p );
                else Parts.Add( (string)c );
            }
        }

        public IDictionary<object, object?> Memory => _memory ?? (_memory = new Dictionary<object, object?>());

        public override string ToString() => Build( new SmarterStringBuilder( new StringBuilder() ) ).ToString();
    }
}
