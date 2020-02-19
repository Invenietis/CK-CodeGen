using System;
using System.Text;

namespace CK.CodeGen.Abstractions
{
    /// <summary>
    /// A code part enables any <see cref="INamedScope"/> to be segmented
    /// in as many parts as needed.
    /// </summary>
    public interface ICodePart<T> : ICodeWriter where T : INamedScope
    {
        /// <summary>
        /// Gets the owner of this part.
        /// </summary>
        T PartOwner { get; }

        /// <summary>
        /// Collects the whole code of this part into a string collector.
        /// </summary>
        /// <param name="b">The string collector where strings will be sent.</param>
        void BuildPart( Action<string> collector );
    }
}
