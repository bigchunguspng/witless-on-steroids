using PF_Bot.Features_Main.Edit.Core;
using PF_Bot.Features_Main.Memes.Core.Options;
using PF_Tools.FFMpeg;

namespace PF_Bot.Features_Main.Edit.Commands.Filter;

public class Randomize : FileEditor_AudioVideoUrl
{
    private const string
        _r_ordered = "o";

    private static readonly Regex
        _r_multipliers    = new(@"(\d{1,2})(?:\*(\d{1,3}))?",         RegexOptions.Compiled), // 1*1
        _r_sfx_pc         = new(@"(\d{1,3})(s)",                      RegexOptions.Compiled), // 10s
        _r_time_pc        = new(@"(\d{1,3})(t)",                      RegexOptions.Compiled), // 10t
        _r_crop_pc        = new(@"(\d{1,3})(x)",                      RegexOptions.Compiled), // 10x
        _r_nuke_pc        = new(@"(\d{1,3})(n)",                      RegexOptions.Compiled), // 10n
        _r_nuke_dep_range = new(@"([1-9])(?:(\.\.)([1-9]))?("")",     RegexOptions.Compiled), // 1..2"
        _r_rep_pc         = new(@"(\d{1,3})(r)",                      RegexOptions.Compiled), // 10r
        _r_rep_range      = new(@"(\d{1,2})(?:(\.\.)(\d{1,2}))?(\^)", RegexOptions.Compiled); // 1..4^

    protected override string SyntaxManual => "/man_random";

    protected override async Task Execute()
    {
        var input = await GetFile();

        // OPTIONS
        var options_ctx = MemeOptionsContext.FromCommandContext(Context);

        var  sfx_pc = options_ctx.GetInt( _r_sfx_pc, 10).ClampByte();
        var time_pc = options_ctx.GetInt(_r_time_pc, 10).ClampByte();
        var crop_pc = options_ctx.GetInt(_r_crop_pc, 10).ClampByte();
        var  rep_pc = options_ctx.GetInt( _r_rep_pc, 10).ClampByte();
        var nuke_pc = options_ctx.GetInt(_r_nuke_pc, 10).ClampByte();

        var  rep_range = options_ctx.GetIntRange(     _r_rep_range, (1, 4), (1, 3));
        var nuke_range = options_ctx.GetIntRange(_r_nuke_dep_range, (1, 2), (1, 3));

        var match = _r_multipliers.Match(options_ctx.Buffer);
        var piece_len = match.ExtractGroup(1, int.Parse, 1);
        var break_len = match.ExtractGroup(2, int.Parse, piece_len);

        var ordered = options_ctx.Check(_r_ordered);

        var filter_options = new RandomizeOptions(rep_range, nuke_range, rep_pc, nuke_pc, sfx_pc, time_pc, crop_pc, ordered);

        var args = Args?.Split()
            .Where(x => x.StartsWith("http").Janai())
            .ToArray();
        args.GetCutTimecodes(out var start, out var length);

        // COOKING
        var sw = Stopwatch.StartNew();

        var (output, probe, options) = await input.InitEditing("RANDOM", Ext);

        var video = probe.GetPrimaryVideoStream();
        if (video != null)
            options.MP4_EnsureSize_Valid_And_Fits(video, 720);

        await new FFMpeg_Effects(input, probe)
            .FX_Random(piece_len / 10.0, break_len / 4.0, filter_options, new TimeSelection(start, length))
            .Out(output, options.Fix_AudioVideo(probe))
            .FFMpeg_Run();

        var log_end = length == TimeSpan.Zero ? probe.Duration : TimeMath.Min(start + length, probe.Duration);

        SendResult(output);
        Log
        (
            $"{Title} >> RANDOMIZE ["
          + $"{piece_len}*{break_len}, "
          + $"R:{ rep_range.from}..{ rep_range.to}^{ rep_pc}%, "
          + $"N:{nuke_range.from}..{nuke_range.to}^{nuke_pc}%, "
          + $"T:{time_pc}%, "
          + $"X:{crop_pc}%, "
          + $"S:{ sfx_pc}%, "
          + $"{start} - {log_end}"
          + $"] >> {sw.ElapsedReadable()}"
        );
    }

    protected override string AudioFileName => $"random_by_piece_fap_bot-{Desert.GetSand()}.mp3";
    protected override string VideoFileName => $"piece_fap_random-{Desert.GetSand()}.mp4";
}