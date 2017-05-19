using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace CK.CodeGen
{
    public struct AssemblyLoadFailure
    {
        public readonly AssemblyName Name;

        public readonly AssemblyName SuccessfulWeakFallback;

        public AssemblyLoadFailure(AssemblyName n, AssemblyName nWeak)
        {
            Name = n;
            SuccessfulWeakFallback = nWeak;
        }
    }
}
