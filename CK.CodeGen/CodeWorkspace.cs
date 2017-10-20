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
        /// <param name="initialSource">Optional initial <see cref="ICodeWorkspace.Global"/> source code.</param>
        /// <param name="assembly">Optional initial <see cref="ICodeWorkspace.AssemblyReferences"/>.</param>
        /// <returns>A new worspace.</returns>
        public static ICodeWorkspace Create( string initialSource = null, params Assembly[] assembly )
        {
            var w = new CodeWorkspaceImpl();
            if( !String.IsNullOrWhiteSpace( initialSource) ) w.Global.Append( initialSource );
            foreach( var a in assembly ) w.DoEnsureAssemblyReference( a );
            return w;
        }
    }
}
