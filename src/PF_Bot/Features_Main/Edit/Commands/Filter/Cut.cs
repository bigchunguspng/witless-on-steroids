using PF_Bot.Features_Main.Edit.Core;
using PF_Tools.FFMpeg;

namespace PF_Bot.Features_Main.Edit.Commands.Filter;

public class Cut : FileEditor_AudioVideoUrl
{
    protected override string SyntaxManual => "/man_cut";

    protected override async Task Execute()
    {
        var args = Args?.Split()
            .SkipWhile(x => x.StartsWith('/') || x.StartsWith("http"))
            .ToArray();

        if (args.GetCutTimecodes(out var start, out var length).Failed())
        {
            SendManual(CUT_MANUAL);
            return;
        }

        var input = await GetFile();

        var (output, probe, options) = await input.InitEditing("Cut", Ext);

        if (start == TimeSpan.Zero && length > probe.Duration)
        {
            options.Options("-c copy");
        }
        else
        {
            var video = probe.GetPrimaryVideoStream();
            if (video != null) options.MP4_EnsureValidSize(video);

            if (start  != TimeSpan.Zero) options.Options($"-ss {start}");
            if (length != TimeSpan.Zero) options.Options($"-t {length}");

            options.Fix_AudioVideo(probe);
        }

        await FFMpeg.Command(input, output, options).FFMpeg_Run();

        SendResult(output);
        Log($"{Title} >> CUT [8K-]");
    }

    protected override string VideoFileName => "piece_fap_bot-cut.mp4";
    protected override string AudioFileName => SongNameOr($"((({Sender}))).mp3");
}