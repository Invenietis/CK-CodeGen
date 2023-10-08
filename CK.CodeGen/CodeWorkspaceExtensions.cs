using CK.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace CK.CodeGen
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
            foreach( var a in others ) @this.DoEnsureAssemblyReference( a );
            return @this;
        }

        /// <summary>
        /// Ensures that this workspace references one or more actual assemblies.
        /// </summary>
        /// <typeparam name="T">Actual type of the workspace.</typeparam>
        /// <param name="this">This workspace.</param>
        /// <param name="assemblies">The assemblies. None of them must be null.</param>
        /// <returns>This workspace to enable fluent syntax.</returns>
        public static T EnsureAssemblyReference<T>( this T @this, IEnumerable<Assembly> assemblies ) where T : ICodeWorkspace
        {
            if( assemblies == null ) throw new ArgumentNullException( nameof( assemblies ) );
            foreach( var a in assemblies ) @this.DoEnsureAssemblyReference( a );
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
            Throw.CheckNotNullArgument( @this );
            Throw.CheckNotNullArgument( t );
            @this.DoEnsureAssemblyReference( t.Assembly );
            foreach( var o in others )
            {
                if( o == null ) throw new ArgumentNullException( nameof(others), "Null type in parameters." );
                @this.DoEnsureAssemblyReference( o.Assembly );
            }
            return @this;
        }

        /// <summary>
        /// Gets the current <see cref="ICodeWorkspace.Global">Global</see> source code.
        /// This is the same as calling ToString() on the Global namespace.
        /// </summary>
        /// <param name="this">This wokspace.</param>
        /// <returns>The current code source for this workspace.</returns>
        public static string GetGlobalSource( this ICodeWorkspace @this )
        {
            Throw.CheckNotNullArgument( @this );
            return @this.Global.Build( new StringBuilder(), true ).ToString();
        }

        /// <summary>
        /// Writes the current <see cref="ICodeWorkspace.Global">Global</see> source code into a <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="this">This wokspace.</param>
        /// <param name="w">Target TextWriter.</param>
        public static void WriteGlobalSource( this ICodeWorkspace @this, TextWriter w )
        {
            Throw.CheckNotNullArgument( @this );
            Throw.CheckNotNullArgument( w );
            @this.Global.Build( w.Write, true );
        }

    }
}
