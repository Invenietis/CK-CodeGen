using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CK.CodeGen;
using System.Diagnostics;
using CK.Text;

namespace CK.CodeGen
{
    abstract class TypeDefinerScopeImpl : NamedScopeImpl, ITypeDefinerScope
    {
        readonly static Regex _variantOutIn = new Regex( @"\b(out|in)\s", RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture );

        readonly Dictionary<string, TypeScopeImpl> _types;

        protected TypeDefinerScopeImpl( CodeWorkspaceImpl workspace, INamedScope? parent )
            : base( workspace, parent )
        {
            _types = new Dictionary<string, TypeScopeImpl>();
        }

        public ITypeScope CreateType( Action<ITypeScope> header )
        {
            if( header == null ) throw new ArgumentNullException( nameof( header ) );
            TypeScopeImpl typeScope = new TypeScopeImpl( Workspace, this );
            header( typeScope );
            typeScope.Initialize();
            _types.Add( typeScope.TypeKey, typeScope );
            Workspace.OnTypeCreated( typeScope );
            return typeScope;
        }

        public ITypeScope FindType( string name )
        {
            if( String.IsNullOrEmpty( name ) ) throw new ArgumentException( "Invalid null or empty type name.", nameof( name ) );
            var m = new StringMatcher( name );
            m.SkipWhiteSpacesAndJSComments();
            if( !m.MatchTypeKey( out string? key ) ) throw new ArgumentException( $"Invalid type name: {name}", nameof( name ) );
            TypeScopeImpl result;
            _types.TryGetValue( key, out result );
            return result;
        }

        public IReadOnlyCollection<ITypeScope> Types => _types.Values;

        protected void MergeTypes( TypeDefinerScopeImpl other )
        {
            foreach( var kv in other._types )
            {
                if( !_types.TryGetValue( kv.Key, out var my ) )
                {
                    my = new TypeScopeImpl( Workspace, this );
                    _types.Add( kv.Key, my );
                }
                my.MergeWith( kv.Value );
            }
        }

        protected SmarterStringBuilder BuildTypes( SmarterStringBuilder b )
        {
            foreach( var t in _types.Values ) t.Build( b, true );
            return b;
        }

        public static string RemoveVariantInOut( string s )
        {
            return _variantOutIn.Replace( s, String.Empty );
        }

        private protected abstract class TypeDefinerPart : CodePart, ITypeDefinerScope
        {
            public TypeDefinerPart( ITypeDefinerScope owner )
                : base( owner )
            {
            }
            ITypeDefinerScope O => (INamespaceScope)base.PartOwner;

            public IReadOnlyCollection<ITypeScope> Types => O.Types;

            public ITypeScope CreateType( Action<ITypeScope> header ) => O.CreateType( header );

            public ITypeScope? FindType( string name ) => O.FindType( name );
        }
    }
}
