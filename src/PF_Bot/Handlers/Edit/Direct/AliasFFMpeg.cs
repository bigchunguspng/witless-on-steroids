using PF_Bot.Handlers.Edit.Direct.Core;

namespace PF_Bot.Handlers.Edit.Direct;

public class AliasFFMpeg : AliasManager
{
    protected override string CMD  => "peg";
    protected override string Code => "ap";
    protected override string Tool => "FFMpeg";

    protected override FilePath Directory => Dir_Alias_Peg;
}