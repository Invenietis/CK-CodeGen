using CK.CodeGen;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CK.CodeGen
{
    /// <summary>
    /// Code modules are optionals. A code module can add source code and/or
    /// rewrite the syntax tree of previous code.
    /// They act as a post processor: their own injected code source (if any) is not processed
    /// by the code module itself.
    /// </summary>
    public interface ICodeGeneratorModule
    {
        /// <summary>
        /// Optionnaly processes the current syntax trees that contain the
        /// initial source code and code added by previous modules.
        /// </summary>
        /// <param name="trees">The syntax trees.</param>
        /// <returns>The syntax trees unchanged or a rewritten one.</returns>
        IReadOnlyList<SyntaxTree> Rewrite( IReadOnlyList<SyntaxTree> trees );

        /// <summary>
        /// Optionnally injects code and/or adds assemblies that must be referenced.
        /// </summary>
        /// <param name="code">A workspace for this module.</param>
        void Inject( ICodeWorkspace code );
    }
}
