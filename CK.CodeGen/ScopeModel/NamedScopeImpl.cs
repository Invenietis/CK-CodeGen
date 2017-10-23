using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CK.CodeGen.Abstractions;
using System.Diagnostics;

namespace CK.CodeGen
{
    abstract class NamedScopeImpl : INamedScope
    {
        readonly CodeWorkspaceImpl _workspace;
        readonly List<string> _code;

        protected NamedScopeImpl( CodeWorkspaceImpl workspace, INamedScope parent )
        {
            _workspace = workspace;
            _code = new List<string>();
            Parent = parent;
            if( parent == null )
            {
                Name = String.Empty;
                FullName = String.Empty;
            }
        }

        public INamedScope Parent { get; }

        ICodeWorkspace INamedScope.Workspace => _workspace;

        internal CodeWorkspaceImpl Workspace => _workspace;

        public string Name { get; private set; }

        public string FullName { get; private set; }

        protected void SetName( string name )
        {
            Debug.Assert( Name == null );
            Debug.Assert( Parent != null );
            Debug.Assert( !String.IsNullOrWhiteSpace( name ) );
            Debug.Assert( NamespaceScopeImpl.RemoveWhiteSpaces( name ) == name );
            Name = name;
            FullName = Parent.Parent != null
                        ? Parent.FullName + '.' + name
                        : name;
        }

        public void DoAdd( string code )
        {
            if( !String.IsNullOrEmpty( code ) ) _code.Add( code );
        }

        internal void MergeWith( NamedScopeImpl other )
        {
            Debug.Assert( other != null );
            _code.AddRange( other._code );
        }

        public abstract StringBuilder Build( StringBuilder b, bool closeScope );

        protected StringBuilder BuildCode( StringBuilder b )
        {
            foreach( var c in _code ) b.Append( c );
            return b;
        }

        protected List<string> Code => _code;

        public static string RemoveWhiteSpaces( string s )
        {
            return Regex.Replace( s, "\\s+", String.Empty, RegexOptions.CultureInvariant );
        }
    }
}
