namespace CK.CodeGen.Abstractions
{
    /// <summary>
    /// Most basic interface: a simple string fragment collector.
    /// </summary>
    public interface ICodeWriter
    {
        /// <summary>
        /// Adds a raw string to this writer.
        /// </summary>
        /// <param name="code">Raw C# code. Can be null or empty.</param>
        void DoAdd( string code );
    }
}
