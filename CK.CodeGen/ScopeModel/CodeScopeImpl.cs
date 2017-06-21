using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using CK.CodeGen.Abstractions;

namespace CK.CodeGen
{
    public abstract class CodeScopeImpl : ICodeScope
    {
        readonly static string TypeKindMissingExFormat = @"The kind of type is missing. Code written: ""{0}"".";
        readonly static string TypeNameMissingExFormat = @"The type name is missing. Code written: ""{0}"".";

        readonly Dictionary<string, TypeScopeImpl> _types;

        protected CodeScopeImpl( ICodeScope parent )
        {
            _types = new Dictionary<string, TypeScopeImpl>();
            Parent = parent;
            Builder = new StringBuilder();
        }

        public StringBuilder Builder { get; }

        public ICodeScope Parent { get; }

        public abstract string Name { get; }

        protected abstract string LocalName { get; }

        public string FullName
        {
            get
            {
                if( Parent == null ) return string.Empty;
                if( Parent.Parent == null ) return LocalName;
                return string.Format( "{0}.{1}", Parent.FullName, LocalName );
            }
        }

        public ITypeScope CreateType( Action<ICodeScope> header )
        {
            TypeScopeImpl typeScope = new TypeScopeImpl( this );
            header( typeScope );
            string typeName = GetTypeName( typeScope );
            typeScope.Initialize( typeName );
            _types.Add( typeName, typeScope );
            return typeScope;
        }

        public static string GetTypeName( ICodeWriter codeWriter )
        {
            string decl = codeWriter.Builder.ToString();
            string kind;
            int startIndex = IndexOfAny( decl, new[] { "class", "interface", "enum", "struct" }, out kind );
            if( startIndex < 0 ) throw new InvalidOperationException( string.Format( TypeKindMissingExFormat, decl ) );
            startIndex += kind.Length + 1;
            if( startIndex >= decl.Length ) throw new InvalidOperationException( string.Format( TypeNameMissingExFormat, decl ) );
            int curr = startIndex;
            while( curr < decl.Length && decl[curr] != '{' && decl[curr] != ':' ) curr++;
            int length = curr - startIndex;
            if( length == 0 ) throw new InvalidOperationException( string.Format( TypeNameMissingExFormat, decl ) );
            string typeName = Regex.Replace( decl.Substring( startIndex, length ), @"(?<!out|in)\s", string.Empty );
            if( typeName == string.Empty ) throw new InvalidOperationException( string.Format( TypeNameMissingExFormat, decl ) );

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

        public ITypeScope FindType( string name )
        {
            TypeScopeImpl result;
            _types.TryGetValue( name, out result );
            return result;
        }

        public abstract IReadOnlyList<ITypeScope> Types { get; }

        public abstract void EnsureUsing( string ns );
    }
}