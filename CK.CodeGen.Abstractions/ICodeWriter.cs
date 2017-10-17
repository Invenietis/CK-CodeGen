namespace CK.CodeGen.Abstractions
{
    /// <summary>
    /// Most basic interface: a simple string fragment collector.
    /// </summary>
    public interface ICodeWriter
    {
        /// <summary>
        /// Appends a raw string to this writer.
        /// </summary>
        /// <param name="code">Raw C# code.</param>
        /// <returns>This writer.</returns>
        ICodeWriter Append( string code );
    }
}
