using System;
using System.Text;

namespace CK.CodeGen
{
    /// <summary>
    /// A code part enables any <see cref="INamedScope"/> to be segmented
    /// in as many parts as needed.
    /// </summary>
    /// <remarks>
    /// This generic interface doesn't extend the non generic <see cref="ICodePart"/>: each
    /// typed code part (<see cref="ITypeScope"/>) can create its own typed part (<see cref="ITypeScopePart"/>)
    /// that needs to be bound to the primary part owner.
    /// </remarks>
    public interface ICodePart<T> : ICodeWriter where T : INamedScope
    {
        /// <summary>
        /// Gets the owner of this part.
        /// </summary>
        T PartOwner { get; }
    }
}
