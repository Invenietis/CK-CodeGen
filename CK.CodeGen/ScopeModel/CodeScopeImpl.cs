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
    abstract class CodeScopeImpl : ICodeScope
    {
        readonly static Regex _variantOutIn = new Regex( @"(out|in)\s", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture );

        readonly CodeWorkspaceImpl _workspace;
        readonly Dictionary<string, TypeScopeImpl> _types;
        readonly List<string> _code;

        protected CodeScopeImpl( CodeWorkspaceImpl workspace, ICodeScope parent )
        {
            _workspace = workspace;
            _types = new Dictionary<string, TypeScopeImpl>();
            _code = new List<string>();
            Parent = parent;
            if( parent == null )
            {
                Name = String.Empty;
                FullName = String.Empty;
            }
        }

        public ICodeScope Parent { get; }

        ICodeWorkspace ICodeScope.Workspace => _workspace;

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

        public ITypeScope CreateType( Action<ITypeScope> header )
        {
            if( header == null ) throw new ArgumentNullException( nameof( header ) );
            TypeScopeImpl typeScope = new TypeScopeImpl( Workspace, this );
            header( typeScope );
            typeScope.Initialize();
            _types.Add( typeScope.Name, typeScope );
            return typeScope;
        }

        public ITypeScope FindType( string name )
        {
            if( String.IsNullOrEmpty( name ) ) throw new ArgumentException( "Invalid null or empty type name.", nameof( name ) );
            TypeScopeImpl result;
            _types.TryGetValue( RemoveWhiteSpaces( RemoveVariantInOut( name ) ), out result );
            return result;
        }

        public IReadOnlyCollection<ITypeScope> Types => _types.Values;

        public void DoAdd( string code )
        {
            if( !String.IsNullOrEmpty( code ) ) _code.Add( code );
        }

        public abstract StringBuilder Build( StringBuilder b, bool closeScope );

        protected StringBuilder BuildCode( StringBuilder b )
        {
            foreach( var c in _code ) b.Append( c );
            b.AppendLine();
            return b;
        }
        protected StringBuilder BuildTypes( StringBuilder b )
        {
            foreach( var t in _types.Values ) t.Build( b, true );
            return b;
        }

        protected List<string> Code => _code;

        public static string RemoveVariantInOut( string s )
        {
            return _variantOutIn.Replace( s, String.Empty );
        }

        public static string RemoveWhiteSpaces( string s )
        {
            return Regex.Replace( s, "\\s+", String.Empty, RegexOptions.CultureInvariant );
        }
    }
}
