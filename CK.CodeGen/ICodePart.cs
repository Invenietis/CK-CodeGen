using System;
using System.Text;

namespace CK.CodeGen
{
    /// <summary>
    /// This non generic interface defines an independent code part that
    /// is not bound to any <see cref="ICodePart{T}.PartOwner"/>.
    /// </summary>
    public interface ICodePart : ICodeWriter, ICodePartFactory
    {
    }
}
