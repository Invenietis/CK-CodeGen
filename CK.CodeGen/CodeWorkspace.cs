using System.Reflection;
using CK.CodeGen.Abstractions;
using System.Collections.Generic;
using System;

namespace CK.CodeGen
{
    /// <summary>
    /// This class exposes the main entry point of the source code model.
    /// </summary>
    public static class CodeWorkspace 
    {
        /// <summary>
        /// Creates a root workspace.
        /// </summary>
        /// <returns>A new empty worspace.</returns>
        public static ICodeWorkspace Create() => new CodeWorkspaceImpl();
    }
}
