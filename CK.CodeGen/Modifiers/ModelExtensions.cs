using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen
{
    public static class ModelExtensions
    {
        public static Modifiers.List<T> Build<T>(this IList<T> @this) => new Modifiers.List<T>(@this);

        public static Modifiers.ClassBuilder Build(this ClassBuilder @this) => new Modifiers.ClassBuilder(@this);

    }
}
