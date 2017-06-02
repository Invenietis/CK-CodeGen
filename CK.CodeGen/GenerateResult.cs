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
        /// Collection of load failures while resolving assembly dependencies.
        /// </summary>
        public readonly IReadOnlyCollection<AssemblyLoadFailure> LoadFailures;

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
        /// Gets whether the the generation succeeds.
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
        public GenerateResult(Exception eE, IReadOnlyList<SyntaxTree> sources, EmitResult r, Assembly a, Exception e, IReadOnlyCollection<AssemblyLoadFailure> f)
        {
            EmitError = eE;
            Assembly = a;
            EmitResult = r;
            Sources = sources;
            AssemblyLoadError = e;
            LoadFailures = f;
        }

        public void LogResult( IActivityMonitor monitor )
        {
            using( monitor.OpenInfo().Send( "Code Generation information." ) )
            {
                if( LoadFailures.Count > 0 )
                {
                    using( monitor.OpenWarn().Send( $"{LoadFailures.Count} assembly load failure(s)." ) )
                        foreach( var e in LoadFailures )
                            if( e.SuccessfulWeakFallback != null ) monitor.Warn().Send( $"'{e.Name}' load failed, used '{e.SuccessfulWeakFallback}' instead." );
                            else monitor.Error().Send( $"'{e.Name}' load failed." );
                }
                if( Success )
                {
                    monitor.Info().Send( "Source code generation and compilation succeeded." );
                    DumpDebugSource( monitor );
                }
                else
                {
                    using( monitor.OpenError().Send( "Generation failed." ) )
                    {
                        if( EmitError != null )
                        {
                            monitor.Error().Send( EmitError );
                        }
                        if( EmitResult != null )
                        {
                            if( !EmitResult.Success )
                            {
                                using( monitor.OpenInfo().Send( $"{EmitResult.Diagnostics.Count()} Compilation diagnostics & Source code." ) )
                                {
                                    foreach( var diag in EmitResult.Diagnostics )
                                    {
                                        monitor.Trace().Send( diag.ToString() );
                                    }
                                }
                            }
                        }
                        DumpDebugSource( monitor );
                    }
                }
                if( AssemblyLoadError != null )
                {
                    monitor.Error().Send( AssemblyLoadError, "Generated assembly load failed." );
                }
                else if( Assembly != null )
                {
                    monitor.Trace().Send( "Generated assembly successfuly loaded." );
                }
            }
        }

        void DumpDebugSource( IActivityMonitor monitor )
        {
            if( Sources != null && Sources.Count > 0 )
            {
                using( monitor.OpenDebug().Send( $"Processed {Sources.Count} source tree(s):" ) )
                {
                    for( int i = 0; i < Sources.Count; ++i )
                    {
                        using( monitor.OpenDebug().Send( $"Source n°{i}" ) )
                        {
                            monitor.Debug().Send( Sources[i].ToString() );
                        }
                    }
                }
            }
        }

        internal GenerateResult WithLoadFailures(IReadOnlyCollection<AssemblyLoadFailure> f) => new GenerateResult(EmitError, Sources, EmitResult, Assembly, AssemblyLoadError, f);
    }
}
