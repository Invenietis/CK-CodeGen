using System;
using System.Collections.Generic;

namespace CK.CodeGen.Abstractions
{
    public interface ICodeScope : ICodeWriter
    {
        ICodeScope Parent { get; }

        string Name { get; }

        string FullName { get; }

        ITypeScope CreateType( Action<ICodeScope> header );

        ITypeScope FindType( string name );

        IReadOnlyList<ITypeScope> Types { get; }

        void AddUsing( string ns );

        IReadOnlyList<string> Usings { get; }
    }
}
