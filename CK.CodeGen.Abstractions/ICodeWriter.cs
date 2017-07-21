namespace CK.CodeGen.Abstractions
{
    public interface ICodeWriter
    {
        ICodeWriter RawAppend( string code );
    }
}
