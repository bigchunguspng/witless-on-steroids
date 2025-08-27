using PF_Bot.Features.Edit.Core;
using PF_Bot.Routing.Commands;
using PF_Bot.Tools_Legacy.FFMpeg;

namespace PF_Bot.Features.Edit.Filter;

public class Slice : AudioVideoUrlCommand
{
    private static readonly Regex _multiplier = new(@"(\d+)(?:\*(\d+))?");

    protected override string SyntaxManual => "/man_slice";

    protected override async Task Execute()
    {
        var (path, waitMessage) = await DownloadFileSuperCool();

        var match = _multiplier.Match(Command!);
        var pacing = match.ExtractGroup(1, int.Parse, 5);
        var breaks = match.ExtractGroup(2, int.Parse, pacing);

        var sw = GetStartedStopwatch();

        if (Type != MediaType.Audio) path = await FFMpegXD.ReduceSize(Origin, path);

        var result = await path.UseFFMpeg(Origin).SliceRandom(breaks / 5D, pacing / 5D).Out("-slices", Ext);

        Bot.DeleteMessageAsync(Chat, waitMessage);

        SendResult(result);
        Log($"{Title} >> SLICE [{breaks}*{pacing}] >> {sw.ElapsedShort()}");
    }

    protected override string AudioFileName { get; } = "sliced_by_piece_fap_bot.mp3";
    protected override string VideoFileName { get; } = "piece_fap_slice.mp4";
}