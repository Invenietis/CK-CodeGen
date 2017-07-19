using System;
using System.Collections.Generic;
using System.Reflection;

namespace CK.CodeGen.Abstractions
{
    public interface ICodeScope : ICodeWriter
    {
        ICodeScope Parent { get; }

        string Name { get; }

        string FullName { get; }

        ITypeScope CreateType( Action<ICodeWriter> header );

        ITypeScope FindType( string name );

        IReadOnlyList<ITypeScope> Types { get; }

        void EnsureUsing( string ns );

        void EnsurePackageReference( string name, string version );

        void EnsureAssemblyReference( Assembly assembly );

        string Build( bool close );
    }
}
