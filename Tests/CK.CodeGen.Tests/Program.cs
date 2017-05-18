#if !NET462
using NUnit.Common;
using NUnitLite;
using System;
using System.Reflection;

namespace CK.CodeGen.Tests
{
    public class Program
    {
        public static int Main(string[] args)
        {
            return new AutoRun(typeof(Program).GetTypeInfo().Assembly)
                .Execute(args, new ExtendedTextWrapper(Console.Out), Console.In);
        }

    }
}
#else
namespace CK.CodeGen.Tests
{
    public class Program
    {
        public static int Main(string[] args)
        {
            return 0;
        }

    }
}
#endif