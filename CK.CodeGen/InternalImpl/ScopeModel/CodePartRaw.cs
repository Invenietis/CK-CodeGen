namespace CK.CodeGen;

sealed class CodePartRaw : BaseCodePart, ICodePart
{
    public ICodePart CreatePart( bool top = false )
    {
        var p = new CodePartRaw();
        if( top ) Parts.Insert( 0, p );
        else Parts.Add( p );
        return p;
    }
}
