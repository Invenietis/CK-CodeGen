using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen
{
    public abstract class TypeMemberBuilder
    {
        protected TypeMemberBuilder(TypeBuilder typeBuilder)
        {
            TypeBuilder = typeBuilder;
        }

        protected TypeBuilder TypeBuilder { get; }

    }
}
