using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CK.CodeGen
{
    public static class ModelExtensions
    {
        public static Modifiers.List<T> Build<T>(this IList<T> @this) => new Modifiers.List<T>(@this);

        public static Modifiers.ClassBuilder Build(this ClassBuilder @this) => new Modifiers.ClassBuilder(@this);

        public static void AddParameter( this IList<ParameterBuilder> @this, ParameterInfo p )
        {
            var pB = new ParameterBuilder();
            pB.Name = p.Name;
            if( p.IsOut ) pB.Attributes.Add( "out" );
            else if( p.ParameterType.IsByRef ) pB.Attributes.Add( "ref" );
            pB.ParameterType = p.ParameterType.IsByRef
                                ? p.ParameterType.GetElementType().ToCSharpName( true )
                                : p.ParameterType.ToCSharpName( true );
            @this.Add( pB );
        }

        public static void AddParameters( this IList<ParameterBuilder> @this, IEnumerable<ParameterInfo> parameters )
        {
            foreach( var p in parameters ) AddParameter( @this, p );
        }

        public static void AddParameters( this IList<ParameterBuilder> @this, MethodInfo m ) => AddParameters( @this, m.GetParameters() );
    }
}
