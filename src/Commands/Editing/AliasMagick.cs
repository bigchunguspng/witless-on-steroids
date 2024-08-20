namespace Witlesss.Commands.Editing;

public class AliasMagick : AliasManager
{
    protected override string CMD  => "im";
    protected override string Code => "ai";
    protected override string Tool => "ImageMagick";
    protected override string Directory => Dir_Alias_Im;
}