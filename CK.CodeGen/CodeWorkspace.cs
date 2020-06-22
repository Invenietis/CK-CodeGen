using System.Reflection;
using CK.CodeGen.Abstractions;
using System.Collections.Generic;
using System;

namespace CK.CodeGen
{
    /// <summary>
    /// This class exposes the main entry points of the source code model.
    /// </summary>
    public static class CodeWorkspace 
    {
        /// <summary>
        /// Gets an empty workspace factory.
        /// </summary>
        public static readonly Func<ICodeWorkspace> Factory = () => Create();

        /// <summary>
        /// Creates a root workspace.
        /// </summary>
        /// <param name="initialSource">Optional initial <see cref="ICodeWorkspace.Global"/> source code.</param>
        /// <param name="assembly">Optional initial <see cref="ICodeWorkspace.AssemblyReferences"/>.</param>
        /// <returns>A new worspace.</returns>
        public static ICodeWorkspace Create( string? initialSource = null, params Assembly[] assembly )
        {
            var w = new CodeWorkspaceImpl();
            if( !String.IsNullOrWhiteSpace( initialSource ) ) w.Global.Append( initialSource );
            foreach( var a in assembly ) w.DoEnsureAssemblyReference( a );
            return w;
        }

        /// <summary>
        /// Creates a new <see cref="ICodeProject"/> with an optional initial code.
        /// </summary>
        /// <param name="projectName">The project name.</param>
        /// <param name="code">Optional code. When null a new empty one is created.</param>
        /// <returns>A new code project.</returns>
        public static ICodeProject CreateProject( string projectName, ICodeWorkspace? code = null )
        {
            if( String.IsNullOrWhiteSpace( projectName ) ) throw new ArgumentNullException( nameof( projectName ) );
            return new CodeProjectImpl( projectName, code ?? CodeWorkspace.Create() );
        }

    }
}
