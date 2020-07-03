using System;
using System.Text;

namespace CK.CodeGen
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
        /// <param name="collector">The string collector where strings will be sent.</param>
        void BuildPart( Action<string> collector );
    }
}
