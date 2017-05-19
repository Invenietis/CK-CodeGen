using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace CK.CodeGen
{
    public class AssemblyClosureResult
    {
        public readonly IReadOnlyCollection<AssemblyLoadFailure> LoadFailures;

        public readonly IEnumerable<Assembly> AllAssemblies;

        public AssemblyClosureResult(IReadOnlyCollection<AssemblyLoadFailure> f, IEnumerable<Assembly> a)
        {
            LoadFailures = f;
            AllAssemblies = a;
        }

    }
}
