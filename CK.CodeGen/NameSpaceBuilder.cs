using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CK.CodeGen
{
    public class NamespaceBuilder
    {
        readonly List<TypeBuilder> _types = new List<TypeBuilder>();

        public NamespaceBuilder(string ns)
        {
            Name = BuildHelpers.CheckValidFullName(ns);
        }

        public List<string> Usings { get; } = new List<string>();

        public IReadOnlyList<TypeBuilder> Types => _types;

        public string Name { get; set; }

        public InterfaceBuilder DefineInterface(string name) => DefineType(name, n => new InterfaceBuilder(this, n));

        public ClassBuilder DefineClass(string name) => DefineType(name, n => new ClassBuilder(this, n));

        public EnumBuilder DefineEnum(string name) => DefineType(name, n => new EnumBuilder(this, n));

        public StructBuilder DefineStruct(string name) => DefineType(name, n => new StructBuilder(this, n));

        T DefineType<T>(string name, Func<string, T> factory) where T : TypeBuilder
        {
            T builder = factory(name);
            _types.Add(builder);
            return builder;
        }

        /// <summary>
        /// Creates constructors that relay calls to public and protected constructors in the base class.
        /// </summary> 
        /// <param name="frontModifiers">Front modifiers</param>
        /// <param name="name">Name of the type.</param>
        /// <param name="baseType">Base type.</param>
        /// <param name="baseConstructorfilter">
        /// Optional predicate used to filter constructors that must be implemented.
        /// When null, all public and protected constructors are public.
        /// </param>
        public ClassBuilder DefineClassWithPublicPassThroughConstructors( string frontModifiers, string name, Type baseType, Func<ConstructorInfo, bool> baseConstructorfilter = null )
        {
            Func<ConstructorInfo, string> filter = null;
            if( baseConstructorfilter != null ) filter = c => baseConstructorfilter( c ) ? "public" : null;
            return DefineClass( name )
                        .Build()
                        .AddFrontModifiers( frontModifiers )
                        .SetBase( baseType )
                        .DefinePassThroughConstructors( filter )
                        .Target;
        }

        public string CreateSource()  => CreateSource( new StringBuilder()).ToString();

        public StringBuilder CreateSource(StringBuilder b)
        {
            b.AppendFormat( $"namespace {Name}" ).AppendLine( "{" );
            BuildUsings( b );
            BuildTypes( b );
            b.Append( "}" );
            return b;
        }

        void BuildUsings(StringBuilder sb)
        {
            var deDup = new HashSet<string>( Usings );
            foreach (string u in deDup )
                if (u.StartsWith("using")) sb.AppendLine(u);
                else sb.Append("using " ).Append( u ).AppendLine(";");
        }

        void BuildTypes(StringBuilder sb)
        {
            foreach (TypeBuilder typeBuilder in _types) typeBuilder.Build(sb);
        }
    }
}
