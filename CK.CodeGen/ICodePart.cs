using System;
using System.Text;

namespace CK.CodeGen
{
    /// <summary>
    /// This non generic interface defines an indepednent code part that
    /// is not bound to any <see cref="ICodePart{T}.PartOwner"/>.
    /// </summary>
    public interface ICodePart : ICodeWriter
    {
        /// <summary>
        /// Collects the whole code of this part into a string collector.
        /// </summary>
        /// <param name="collector">The string collector where strings will be sent.</param>
        void BuildPart( Action<string> collector );

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
