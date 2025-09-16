using PF_Bot.Core.Editing;
using PF_Bot.Handlers.Edit.Core;
using PF_Bot.Handlers.Edit.Shared;
using PF_Tools.FFMpeg;

namespace PF_Bot.Handlers.Edit.Filter;

public class Slice : AudioVideoUrlCommand
{
    private static readonly Regex _multiplier = new(@"(\d+)(?:\*(\d+))?");

    protected override string SyntaxManual => "/man_slice";

    protected override async Task Execute()
    {
        var (input, waitMessage) = await DownloadFileSuperCool();

        var match = _multiplier.Match(Command!);
        var pacing = match.ExtractGroup(1, int.Parse, 5);      // length of shown   parts
        var breaks = match.ExtractGroup(2, int.Parse, pacing); // length of dropped parts

        var sw = Stopwatch.StartNew();

        var (output, probe, options) = await input.InitEditing("Slice", Ext);

        var video = probe.GetPrimaryVideoStream();
        if (video != null)
            options.MP4_EnsureSize_Valid_And_Fits(video, 720);

        await new FFMpeg_Slice(input, probe)
            .ApplyRandomSlices(breaks / 5D, pacing / 5D)
            .Out(output, options.Fix_AudioVideo(probe))
            .FFMpeg_Run();

        Bot.DeleteMessageAsync(Chat, waitMessage);

        SendResult(output);
        Log($"{Title} >> SLICE [{breaks}*{pacing}] >> {sw.ElapsedReadable()}");
    }

    protected override string AudioFileName { get; } = "sliced_by_piece_fap_bot.mp3";
    protected override string VideoFileName { get; } = "piece_fap_slice.mp4";
}