using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CK.CodeGen.Abstractions;

namespace CK.CodeGen
{
    sealed class TypeScopeImpl : CodeScopeImpl, ITypeScope
    {
        readonly static string TypeKindMissingExFormat = @"The kind of type is missing. Code written: ""{0}"".";
        readonly static string TypeNameMissingExFormat = @"The type name is missing. Code written: ""{0}"".";

        string _name;

        internal TypeScopeImpl( ICodeScope parent )
            : base( parent )
        {
        }

        public override string Name
        {
            get
            {
                Debug.Assert( !string.IsNullOrWhiteSpace( _name ) );
                return _name;
            }
        }

        protected override string LocalName => Name;

        public override ICodeScope EnsureUsing( string ns )
        {
            Parent.EnsureUsing( ns );
            return this;
        }

        public override ICodeScope EnsurePackageReference( string name, string version )
        {
            Parent.EnsurePackageReference( name, version );
            return this;
        }

        public override ICodeScope EnsureAssemblyReference( Assembly assembly )
        {
            Parent.EnsureAssemblyReference( assembly );
            return this;
        }

        internal void InitializeHeader( Action<ITypeScope> header )
        {
            header( this );
            bool hasOpenBrace;
            _name = GetTypeName( out hasOpenBrace );
            if( !hasOpenBrace ) RawAppend( "{" );
        }

        /// <summary>
        /// Extracts header (front modifiers, type name, generic parameters, base class, interfaces and generic constraints.)
        /// </summary>
        /// <param name="rest">Everything after open brace.</param>
        /// <returns>the header.</returns>
        string GetTypeName( out bool hasOpenBrace )
        {
            hasOpenBrace = false;
            string decl = Build( false );
            string kind;
            int startIndex = IndexOfAny( decl, new[] { "class", "interface", "enum", "struct" }, out kind );
            if( startIndex < 0 ) throw new InvalidOperationException( string.Format( TypeKindMissingExFormat, decl ) );
            startIndex += kind.Length + 1;
            if( startIndex >= decl.Length ) throw new InvalidOperationException( string.Format( TypeNameMissingExFormat, decl ) );
            int curr = startIndex;
            while( curr < decl.Length && decl[curr] != '{' && decl[curr] != ':' ) curr++;
            if( curr < decl.Length && decl[curr] == '{' ) hasOpenBrace = true;
            int length = curr - startIndex;
            if( length == 0 ) throw new InvalidOperationException( string.Format( TypeNameMissingExFormat, decl ) );
            string typeName = Regex.Replace( decl.Substring( startIndex, length ), @"(?<!out|in)\s", string.Empty );
            if( typeName == string.Empty ) throw new InvalidOperationException( string.Format( TypeNameMissingExFormat, decl ) );

            while( curr < decl.Length && decl[curr] != '{' ) curr++;
            if( curr < decl.Length && decl[curr] == '{' ) hasOpenBrace = true;

            return typeName;
        }

        static int IndexOfAny( string s, IEnumerable<string> values, out string found )
        {
            found = null;

            foreach( string value in values )
            {
                int idx = s.IndexOf( value );
                if( idx >= 0 )
                {
                    found = value;
                    return idx;
                }
            }

            return -1;
        }

        public override string Build( bool close ) => Build( new StringBuilder(), close );

        internal string Build(StringBuilder sb, bool close)
        {
            foreach( string code in Code ) sb.Append( code );
            if( close ) sb.Append( "}" );

            return sb.ToString();
        }
    }
}
