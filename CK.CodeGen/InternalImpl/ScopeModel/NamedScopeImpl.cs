using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CK.CodeGen;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace CK.CodeGen
{
    abstract class NamedScopeImpl : INamedScope
    {
        string? _name; 
        string? _fullName; 
        readonly CodeWorkspaceImpl _workspace;
        private protected readonly CodePart CodePart;

        private protected NamedScopeImpl( CodeWorkspaceImpl workspace, INamedScope? parent )
        {
            _workspace = workspace;
            CodePart = new CodePart( this );
            Parent = parent;
            if( parent == null )
            {
                _name = String.Empty;
                _fullName = String.Empty;
            }
        }

        public INamedScope? Parent { get; }

        ICodeWorkspace INamedScope.Workspace => _workspace;

        internal CodeWorkspaceImpl Workspace => _workspace;

        public string Name => _name!;

        public string FullName => _fullName!;

        private protected void SetName( string name )
        {
            Debug.Assert( _name == null );
            Debug.Assert( Parent != null );
            Debug.Assert( !String.IsNullOrWhiteSpace( name ) );
            _name = name;
            _fullName = Parent.Parent != null
                        ? Parent.FullName + '.' + name
                        : name;
        }

        
        public void DoAdd( string? code ) => CodePart.DoAdd( code );

        public IDictionary<object, object?> Memory => CodePart.Memory;

        public void Build( Action<string> collector, bool closeScope ) => Build( new SmarterStringBuilder( collector ), closeScope );

        public StringBuilder Build( StringBuilder b, bool closeScope ) => Build( new SmarterStringBuilder( b ), closeScope ).Builder!;

        internal protected abstract SmarterStringBuilder Build( SmarterStringBuilder b, bool closeScope );

        public override string ToString() => Build( new StringBuilder(), true ).ToString();

        public static string RemoveWhiteSpaces( string s )
        {
            return Regex.Replace( s, "\\s+", String.Empty, RegexOptions.CultureInvariant );
        }
    }
}
