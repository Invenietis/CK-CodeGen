using System;
using System.Collections.Generic;
using System.Text;

namespace CK.CodeGen
{
    /// <summary>
    /// Configures the behavior of helper methods regarding access protection keywords.
    /// </summary>
    public enum AccessProtectionOption
    {
        /// <summary>
        /// Protection access keywords are ignored.
        /// </summary>
        None,

        /// <summary>
        /// Internal protection must be ignored.
        /// </summary>
        RemoveInternal,

        /// <summary>
        /// Throws an <see cref="ArgumentException"/> if the protection is "internal" or "private protected".
        /// </summary>
        ThrowOnPureInternal,

        /// <summary>
        /// All protection access keywords are kept.
        /// </summary>
        All
    }
}
