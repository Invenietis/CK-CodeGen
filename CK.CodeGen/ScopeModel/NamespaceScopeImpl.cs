using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using CK.CodeGen.Abstractions;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Linq;

namespace CK.CodeGen
{
    sealed class NamespaceScopeImpl : TypeDefinerScopeImpl, INamespaceScope
    {
        readonly static Regex _nsName = new Regex( @"^\s*(?<1>\w+)(\s*\.\s*(?<1>\w+))*\s*$", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture );
        readonly Dictionary<string, KeyValuePair<string, string>> _usings;
        readonly List<NamespaceScopeImpl> _subNamespaces;

        internal NamespaceScopeImpl( CodeWorkspaceImpl ws, INamespaceScope parent, string name )
            : base( ws, parent )
        {
            Debug.Assert( (parent == null) == (name == "") );
            Debug.Assert( parent == null || parent is NamespaceScopeImpl );
            _usings = new Dictionary<string, KeyValuePair<string, string>>();
            _subNamespaces = new List<NamespaceScopeImpl>();
            if( parent != null ) SetName( name );
        }

        internal void MergeWith( NamespaceScopeImpl other )
        {
            Debug.Assert( other != null );
            foreach( var u in other._usings )
            {
                DoEnsureUsing( u.Key, u.Value.Key, u.Value.Value );
            }
            CodePart.MergeWith( other.CodePart );
            MergeTypes( other );
            foreach( var oNS in other._subNamespaces )
            {
                var my = _subNamespaces.FirstOrDefault( x => x.Name == oNS.Name );
                if( my == null )
                {
                    my = new NamespaceScopeImpl( Workspace, this, oNS.Name );
                    _subNamespaces.Add( my );
                }
                my.MergeWith( oNS );
            }
        }

        INamespaceScope INamespaceScope.Parent => Parent;

        internal new NamespaceScopeImpl Parent => (NamespaceScopeImpl)base.Parent;

        public INamespaceScope EnsureUsing( string ns )
        {
            if( !String.IsNullOrWhiteSpace( ns ) )
            {
                int assignIdx = ns.IndexOf( '=' );
                if( assignIdx != 0 )
                {
                    if( assignIdx > 0 )
                    {
                        var def = ns.Substring( assignIdx + 1 );
                        ns = CheckAndNormalizeOneName( ns.Substring( 0, assignIdx ) );
                        return DoEnsureUsing( ns, def );
                    }
                    return DoEnsureUsing( CheckAndNormalizeNamespace( ns ), null );
                }
            }
            throw new ArgumentException( $"'using {ns}' is invalid.", nameof( ns ) );
        }

        public INamespaceScope EnsureUsingAlias( string alias, string definition )
        {
            if( String.IsNullOrWhiteSpace( definition ) ) throw new ArgumentException( $"'{definition}' is not a valid alias definition.", nameof( definition ) );
            return DoEnsureUsing( CheckAndNormalizeOneName( alias ), definition );
        }

        INamespaceScope DoEnsureUsing( string alias, string definition )
        {
            Debug.Assert(
                    (definition == null && CheckAndNormalizeNamespace( alias ) == alias)
                    || (definition != null && CheckAndNormalizeOneName( alias ) == alias)
                );
            var keyDef = definition;
            if( keyDef != null )
            {
                // We must normalize the trailing ;.
                definition = definition.Trim();
                if( definition.Length == 0 || definition == ";" ) throw new ArgumentException( $"'{definition}' is not a valid alias definition.", nameof( definition ) );
                if( definition[definition.Length - 1] != ',' ) definition += ';';

                keyDef = RemoveWhiteSpaces( definition );
            }
            return DoEnsureUsing( alias, keyDef, definition );
        }
        INamespaceScope DoEnsureUsing( string alias, string keyDef, string definition )
        {
            Debug.Assert( alias != null );
            Debug.Assert( (keyDef == null) == (definition == null) );
            if( _usings.TryGetValue( alias, out var defs ) )
            {
                if( defs.Key == keyDef ) return this;
                string existing = defs.Value != null ? " = " + defs.Value : ";";
                string newOne = definition != null ? " = " + definition : ";";
                throw new ArgumentException( $"using {alias}{newOne} is already defined in this scope as: {alias}{existing}." );
            }
            var scope = this;
            while( (scope = scope.Parent) != null )
            {
                if( scope._usings.TryGetValue( alias, out defs ) && defs.Key == keyDef ) return this;
            }
            _usings.Add( alias, new KeyValuePair<string, string>( keyDef, definition ) );
            return this;
        }

        static public string[] CheckAndGetNamespaceParts( string ns )
        {
            if( ns == null ) throw new ArgumentNullException( ns );
            var m = _nsName.Match( ns );
            if( !m.Success ) throw new ArgumentException( $"Invalid namespace: {ns}" );
            var captures = m.Groups[1].Captures;
            var result = new string[captures.Count];
            for( int i = 0; i < result.Length; ++i ) result[i] = captures[i].Value;
            return result;
        }

        static string CheckAndNormalizeNamespace( string ns )
        {
            return String.Join( ".", CheckAndGetNamespaceParts( ns ) );
        }

        static public string CheckAndNormalizeOneName( string ns )
        {
            var n = CheckAndGetNamespaceParts( ns );
            if( n.Length != 1 ) throw new ArgumentException( $"Only one identifier is expected: {ns}." );
            return n[0];
        }

        public INamespaceScope FindOrCreateNamespace( string ns )
        {
            string[] names = CheckAndGetNamespaceParts( ns );
            return DoFindOrCreateNamespace( names, 0 );
        }

        INamespaceScope DoFindOrCreateNamespace( string[] names, int idx )
        {
            var exist = _subNamespaces.FirstOrDefault( x => x.Name == names[idx] );
            if( exist == null )
            {
                exist = new NamespaceScopeImpl( Workspace, this, names[idx] );
                _subNamespaces.Add( exist );
            }
            return names.Length == ++idx ? exist : exist.DoFindOrCreateNamespace( names, idx );
        }

        public IReadOnlyCollection<INamespaceScope> Namespaces => _subNamespaces;

        internal protected override SmarterStringBuilder Build( SmarterStringBuilder b, bool closeScope )
        {
            b.AppendLine();
            if( Workspace.Global != this ) b.Append( "namespace " )
                                            .Append( Name )
                                            .AppendLine()
                                            .Append( '{' )
                                            .AppendLine();
            foreach( var e in _usings )
            {
                b.AppendLine().Append( "using " ).Append( e.Key );
                if( e.Value.Value == null ) b.Append( ';' );
                else b.Append( " = " ).Append( e.Value.Value );
                b.AppendLine();
            }
            CodePart.Build( b );
            foreach( var ns in _subNamespaces )
            {
                ns.Build( b, true );
            }
            BuildTypes( b );
            if( Workspace.Global != this && closeScope ) b.AppendLine().Append( "}" );
            return b;
        }

        public INamespaceScopePart CreatePart( bool top )
        {
            var p = new Part( this );
            if( top ) CodePart.Parts.Insert( 0, p );
            else CodePart.Parts.Add( p );
            return p;
        }

        class Part : TypeDefinerPart, INamespaceScopePart
        {
            public Part( INamespaceScope owner )
                : base( owner )
            {
            }

            public new INamespaceScope PartOwner => (INamespaceScope)base.PartOwner;

            public INamespaceScope Parent => PartOwner.Parent;

            public IReadOnlyCollection<INamespaceScope> Namespaces => PartOwner.Namespaces;

            public INamespaceScopePart CreatePart( bool top )
            {
                var p = new Part( this );
                if( top ) Parts.Insert( 0, p );
                else Parts.Add( p );
                return p;
            }

            public INamespaceScope EnsureUsing( string ns ) => PartOwner.EnsureUsing( ns );

            public INamespaceScope EnsureUsingAlias( string alias, string definition ) => PartOwner.EnsureUsingAlias( alias, definition );

            public INamespaceScope FindOrCreateNamespace( string ns ) => PartOwner.FindOrCreateNamespace( ns );

        }

    }
}
