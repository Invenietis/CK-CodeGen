using System.Text;

namespace CK.CodeGen.Abstractions
{
    public interface ICodeWriter
    {
        StringBuilder Builder { get; }
    }
}
