using System;
using System.Collections.Generic;

namespace CK.CodeGen.Abstractions
{
    public interface ICodeScope : ICodeWriter
    {
        ICodeScope Parent { get; }

        string Name { get; }

        ITypeScope CreateType( Action<ICodeWriter> header );

        IReadOnlyList<ITypeScope> TypeScopes { get; }

        void AddUsing( string ns );

        IReadOnlyList<string> Usings { get; }
    }
}
