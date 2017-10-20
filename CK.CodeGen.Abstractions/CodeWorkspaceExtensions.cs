using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace CK.CodeGen.Abstractions
{
    /// <summary>
    /// Provides extension methods on <see cref="ICodeWorkspace"/>.
    /// </summary>
    public static class CodeWorkspaceExtensions
    {
        /// <summary>
        /// Ensures that this workspace references one or more actual assemblies.
        /// </summary>
        /// <typeparam name="T">Actual type of the workspace.</typeparam>
        /// <param name="this">This workspace.</param>
        /// <param name="assembly">The assembly. Must not be null.</param>
        /// <param name="others">Optional other assemblies to reference. None of them must be null.</param>
        /// <returns>This workspace to enable fluent syntax.</returns>
        public static T EnsureAssemblyReference<T>( this T @this, Assembly assembly, params Assembly[] others ) where T : ICodeWorkspace
        {
            if( assembly == null ) throw new ArgumentNullException( nameof( assembly ) );
            @this.DoEnsureAssemblyReference( assembly );
            if( others == null ) throw new ArgumentNullException( nameof( others ) );
            foreach( var a in others ) @this.DoEnsureAssemblyReference( a );
            return @this;
        }

        /// <summary>
        /// Ensures that this workspace references the given type's assembly.
        /// </summary>
        /// <typeparam name="T">Actual type of the workspace.</typeparam>
        /// <param name="this">This workspace.</param>
        /// <param name="others">Optional other types whose assemblies must be referenced. None of them must be null.</param>
        /// <param name="t">The type whose assembly must be referenced. Must not be null.</param>
        /// <returns>This workspace to enable fluent syntax.</returns>
        public static T EnsureAssemblyReference<T>( this T @this, Type t, params Type[] others ) where T : ICodeWorkspace
        {
            if( t == null ) throw new ArgumentNullException( nameof( t ) );
            @this.DoEnsureAssemblyReference( t.Assembly );
            if( others == null ) throw new ArgumentNullException( nameof( others ) );
            foreach( var o in others )
            {
                if( o == null ) throw new ArgumentNullException( nameof(others), "Null type in parameters." );
                @this.DoEnsureAssemblyReference( o.Assembly );
            }
            return @this;
        }

        /// <summary>
        /// Gets the current <see cref="ICodeWorkspace.Global">Global</see> source code.
        /// </summary>
        /// <param name="this">This wokspace.</param>
        /// <returns>The current code source for this workspace.</returns>
        public static string GetGlobalSource( this ICodeWorkspace @this )
        {
            return @this.Global.Build( new StringBuilder(), true ).ToString();
        }

    }
}
