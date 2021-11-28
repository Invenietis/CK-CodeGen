using System;
using System.Text;

namespace CK.CodeGen
{
    /// <summary>
    /// This non generic interface defines an independent code part that
    /// is not bound to any <see cref="ICodePart{T}.PartOwner"/>.
    /// </summary>
    public interface ICodePart : ICodeWriter
    {
        /// <summary>
        /// Creates a segment of code inside this code.
        /// </summary>
        /// <param name="top">
        /// Optionally creates the new part at the start of the code instead of at the
        /// current writing position in the code.
        /// </param>
        /// <returns>The code part to use.</returns>
        ICodePart CreatePart( bool top = false );
    }
}
