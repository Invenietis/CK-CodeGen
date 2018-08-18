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
        /// Collects the whole code of this part into a <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="b">The string builder to write to.</param>
        /// <returns>The string builder.</returns>
        StringBuilder BuildPart( StringBuilder b );
    }
}
