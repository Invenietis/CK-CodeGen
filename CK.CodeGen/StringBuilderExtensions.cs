using System.Text;

namespace CK.CodeGen
{
    static class StringBuilderExtensions
    {
        internal static StringBuilder AppendWithWhitespace(this StringBuilder @this, object arg) => @this.AppendFormat("{0} ", arg);
    }
}
