using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CK.CodeGen.Abstractions;

namespace CK.CodeGen
{
    public abstract class CodeScopeImpl : ICodeScope
    {
        readonly Dictionary<string, TypeScopeImpl> _types;
        readonly List<string> _code;

        protected CodeScopeImpl( ICodeScope parent )
        {
            _types = new Dictionary<string, TypeScopeImpl>();
            _code = new List<string>();
            Parent = parent;
        }

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

        public ITypeScope CreateType( Action<ITypeScope> header )
        {
            if( header == null ) throw new ArgumentNullException( nameof( header ) );
            TypeScopeImpl typeScope = new TypeScopeImpl( this );
            typeScope.InitializeHeader( header );
            _types.Add( typeScope.Name, typeScope );
            return typeScope;
        }

        public ITypeScope FindType( string name )
        {
            TypeScopeImpl result;
            _types.TryGetValue( name, out result );
            return result;
        }

        public IReadOnlyList<ITypeScope> Types => _types.Values.ToList();

        public abstract ICodeScope EnsureUsing( string ns );

        public abstract ICodeScope EnsurePackageReference( string name, string version );

        public abstract ICodeScope EnsureAssemblyReference( Assembly assembly );

        public ICodeWriter RawAppend( string code )
        {
            _code.Add( code );
            return this;
        }

        public abstract string Build( bool close );

        protected List<string> Code => _code;
    }
}