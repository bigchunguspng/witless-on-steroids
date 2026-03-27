using PF_Bot.Features_Main.Edit.Core;
using PF_Bot.Features_Main.Memes.Core.Options;
using PF_Tools.FFMpeg;

namespace PF_Bot.Features_Main.Edit.Commands.Filter;

/*
    /random[ops] [timecodes]
    == fragment repeats
    ^N - % of repeating a fragment 0..100,10
    *N..M - range of possible repeat number 1..99,1..4
    == nuke
    N% - % of nuking a fragment, 0..100,10
    N" / N..M" - range of possible nuke depth, 1..9,1..2
    == misc
    s - sorted (fragments are shuffled a bit by default)
*/

public class Randomize : FileEditor_AudioVideoUrl
{
    private const string
        _r_sorted = "s";

    private static readonly Regex
        _r_nuke_pc        = new(@"(\d{1,3})(%)",                      RegexOptions.Compiled),
        _r_nuke_dep_range = new(@"([1-9])(?:(\.\.)([1-9]))?("")",     RegexOptions.Compiled),
        _r_rep_pc         = new(@"(\^)(\d{1,3})(%)",                  RegexOptions.Compiled),
        _r_rep_range      = new(@"(\*)(\d{1,2})(?:(\.\.)(\d{1,2}))?", RegexOptions.Compiled);

    protected override string SyntaxManual => "/man_random";

    protected override async Task Execute()
    {
        var input = await GetFile();

        // OPTIONS
        var options_ctx = MemeOptionsContext.FromCommandContext(Context);

        var  rep_pc = options_ctx.GetInt( _r_rep_pc, 10, 2).ClampByte();
        var nuke_pc = options_ctx.GetInt(_r_nuke_pc, 10   ).ClampByte();

        var  rep_range = options_ctx.GetIntRange(     _r_rep_range, (1, 4), (2, 4));
        var nuke_range = options_ctx.GetIntRange(_r_nuke_dep_range, (1, 2), (1, 3));

        var sorted = options_ctx.Check(_r_sorted);

        var filter_options = new RandomizeOptions(rep_range, nuke_range, rep_pc, nuke_pc, sorted);

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

        await new FFMpeg_Randomize(input, probe)
            .ApplyEffects(filter_options, new TimeSelection(start, length))
            .Out(output, options.Fix_AudioVideo(probe))
            .FFMpeg_Run();

        SendResult(output);
        Log
        (
            $"{Title} >> RANDOMIZE ["
          + $"R:{ rep_range.from}..{ rep_range.to}^{ rep_pc}%, "
          + $"N:{nuke_range.from}..{nuke_range.to}^{nuke_pc}%, "
          + $"{start} - {TimeMath.Min(length, probe.Duration)}"
          + $"] >> {sw.ElapsedReadable()}"
        );
    }

    protected override string AudioFileName { get; } = "random_by_piece_fap_bot.mp3";
    protected override string VideoFileName { get; } = "piece_fap_random.mp4";
}