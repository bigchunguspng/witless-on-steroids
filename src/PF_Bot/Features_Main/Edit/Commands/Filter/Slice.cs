using PF_Bot.Features_Main.Edit.Core;
using PF_Tools.FFMpeg;

namespace PF_Bot.Features_Main.Edit.Commands.Filter;

public class Slice : FileEditor_AudioVideoUrl
{
    private static readonly Regex
        _rgx_multipliers = new(@"(\d{1,2})(?:\*(\d{1,3}))?", RegexOptions.Compiled);

    protected override string SyntaxManual => "/man_slice";

    protected override async Task Execute()
    {
        var input = await GetFile();

        var match = _rgx_multipliers.Match(Options);
        var piece_len = match.ExtractGroup(1, int.Parse, 5);
        var break_len = match.ExtractGroup(2, int.Parse, piece_len);

        var args = Args?.Split()
            .Where(x => x.StartsWith("http").Janai())
            .ToArray();
        args.GetCutTimecodes(out var start, out var length);

        var sw = Stopwatch.StartNew();

        var (output, probe, options) = await input.InitEditing("Slice", Ext);

        var video = probe.GetPrimaryVideoStream();
        if (video != null)
            options.MP4_EnsureSize_Valid_And_Fits(video, 720);

        await new FFMpeg_Effects(input, probe)
            .FX_Slice(piece_len / 5.0, break_len / 5.0, new TimeSelection(start, length))
            .Out(output, options.Fix_AudioVideo(probe))
            .FFMpeg_Run();

        var log_end = length == TimeSpan.Zero ? probe.Duration : TimeMath.Min(start + length, probe.Duration);

        SendResult(output);
        Log($"{Title} >> SLICE [{piece_len}*{break_len}, {start} - {log_end}] >> {sw.ElapsedReadable()}");
    }

    protected override string AudioFileName => $"sliced_by_piece_fap_bot-{Desert.GetSand()}.mp3";
    protected override string VideoFileName => $"piece_fap_slice-{Desert.GetSand()}.mp4";
}