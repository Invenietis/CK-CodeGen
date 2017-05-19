using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
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
        /// <param name="r">Rosely result.</param>
        /// <param name="a">Loaded assembly if any.</param>
        /// <param name="e">Load error if any.</param>
        public GenerateResult(Exception eE, EmitResult r, Assembly a, Exception e, IReadOnlyCollection<AssemblyLoadFailure> f)
        {
            EmitError = eE;
            Assembly = a;
            EmitResult = r;
            AssemblyLoadError = e;
            LoadFailures = f;
        }

        internal GenerateResult WithLoadFailures(IReadOnlyCollection<AssemblyLoadFailure> f) => new GenerateResult(EmitError, EmitResult, Assembly, AssemblyLoadError, f);
    }
}
