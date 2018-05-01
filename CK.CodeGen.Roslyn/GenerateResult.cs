using CK.Core;
using CK.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace CK.CodeGen
{
    /// <summary>
    /// Captures generation result.
    /// </summary>
    public struct GenerateResult
    {
        /// <summary>
        /// The loaded assembly (optional).
        /// </summary>
        public readonly Assembly Assembly;

        /// <summary>
        /// List of <see cref="AssemblyLoadConflict"/> that occured while
        /// resolving assembly dependencies.
        /// </summary>
        public readonly IReadOnlyCollection<AssemblyLoadConflict> LoadConflicts;

        /// <summary>
        /// List of final Syntax trees that have been compiled.
        /// </summary>
        public readonly IReadOnlyList<SyntaxTree> Sources;

        /// <summary>
        /// The Roselyn result.
        /// </summary>
        public readonly EmitResult EmitResult;

        /// <summary>
        /// Error raised by the emit processus itself.
        /// </summary>
        public readonly Exception EmitError;

        /// <summary>
        /// Error resulting from the attempt to load the generated <see cref="Assembly"/> if any.
        /// </summary>
        public readonly Exception AssemblyLoadError;

        /// <summary>
        /// Gets whether the generation succeeds.
        /// </summary>
        public bool Success => EmitResult?.Success == true && AssemblyLoadError == null;

        /// <summary>
        /// Initializes a new result.
        /// </summary>
        /// <param name="eE">Emit exception.</param>
        /// <param name="sources">Sources.</param>
        /// <param name="r">Rosely result.</param>
        /// <param name="a">Loaded assembly if any.</param>
        /// <param name="e">Load error if any.</param>
        /// <param name="f">Load failures.</param>
        public GenerateResult( Exception eE, IReadOnlyList<SyntaxTree> sources, EmitResult r, Assembly a, Exception e, IReadOnlyList<AssemblyLoadConflict> f )
        {
            EmitError = eE;
            Assembly = a;
            EmitResult = r;
            Sources = sources;
            AssemblyLoadError = e;
            LoadConflicts = f;
        }

        /// <summary>
        /// Dumps the result of the compilation into a monitor.
        /// </summary>
        /// <param name="monitor">The monitor to use.</param>
        /// <param name="dumpSourceLevel">Optionnaly dumps the source as another <see cref="CK.Core.LogLevel"/>.</param>
        public void LogResult( IActivityMonitor monitor, LogLevel dumpSourceLevel = LogLevel.Debug )
        {
            if( monitor == null ) throw new ArgumentNullException( nameof( monitor ) );
            using( monitor.OpenInfo( "Code Generation information." ) )
            {
                if( LoadConflicts.Count > 0 )
                {
                    using( monitor.OpenWarn( $"{LoadConflicts.Count} assembly load conflict(s)." ) )
                    {
                        foreach( var e in LoadConflicts )
                        {
                            if( e.Resolved != null )
                            {
                                monitor.Warn( e.ToString() );
                            }
                            else
                            {
                                monitor.Error( e.ToString() );
                            }
                        }
                    }
                }
                if( Success )
                {
                    monitor.Info( "Source code generation and compilation succeeded." );
                    DumpSources( monitor, dumpSourceLevel );
                }
                else
                {
                    using( monitor.OpenError( "Generation failed." ) )
                    {
                        if( EmitError != null )
                        {
                            monitor.Error( EmitError );
                        }
                        if( EmitResult != null )
                        {
                            if( !EmitResult.Success )
                            {
                                using( monitor.OpenInfo( $"{EmitResult.Diagnostics.Count()} Compilation diagnostics." ) )
                                {
                                    foreach( var diag in EmitResult.Diagnostics )
                                    {
                                        monitor.Trace( diag.ToString() );
                                    }
                                }
                            }
                        }
                        DumpSources( monitor, dumpSourceLevel );
                    }
                }
                if( AssemblyLoadError != null )
                {
                    monitor.Error( "Generated assembly load failed.", AssemblyLoadError );
                }
                monitor.CloseGroup( Assembly != null
                                            ? "Generated assembly successfuly loaded."
                                            : (Success ? "Succeeded." : "Failed.") );
            }
        }

        void DumpSources( IActivityMonitor monitor, LogLevel level )
        {
            if( Sources != null && Sources.Count > 0 )
            {
                using( monitor.OpenGroup( level, $"Processed {Sources.Count} source tree(s):" ) )
                {
                    for( int i = 0; i < Sources.Count; ++i )
                    {
                        using( monitor.OpenGroup( level, $"Source n°{i}" ) )
                        {
                            monitor.Log( level, Sources[i].ToString() );
                        }
                    }
                }
            }
        }

        internal GenerateResult WithLoadFailures( IReadOnlyList<AssemblyLoadConflict> f ) => new GenerateResult( EmitError, Sources, EmitResult, Assembly, AssemblyLoadError, f );
    }
}