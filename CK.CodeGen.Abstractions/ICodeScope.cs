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

        ITypeScope CreateType( Action<ITypeScope> header );

        ITypeScope FindType( string name );

        IReadOnlyList<ITypeScope> Types { get; }

        ICodeScope EnsureUsing( string ns );

        ICodeScope EnsurePackageReference( string name, string version );

        ICodeScope EnsureAssemblyReference( Assembly assembly );

        string Build( bool close );
    }
}
