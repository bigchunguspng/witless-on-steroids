using PF_Bot.Features.Edit.Direct.Core;

namespace PF_Bot.Features.Edit.Direct;

public class AliasFFMpeg : AliasManager
{
    protected override string CMD  => "peg";
    protected override string Code => "ap";
    protected override string Tool => "FFMpeg";
    protected override string Directory => Dir_Alias_Peg;
}