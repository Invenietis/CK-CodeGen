using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CK.CodeGen
{
    public interface ICodeGeneratorModule
    {
        IEnumerable<Assembly> RequiredAssemblies { get; }

        void AppendSource( StringBuilder b );

        SyntaxTree PostProcess( SyntaxTree t );
    }
}
