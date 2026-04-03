using PF_Bot.Features_Main.Edit.Core;
using PF_Tools.FFMpeg;

namespace PF_Bot.Features_Main.Edit.Commands.Filter;

public class Cut : FileEditor_AudioVideoUrl
{
    protected override string SyntaxManual => "/man_cut";

    protected override async Task Execute()
    {
        var args_split = Args?.Split();
        var args = args_split?
            .Where(x => x.StartsWith("http").Janai())
            .ToArray();

        var byURL = args_split?.Any(x => x.StartsWith("http")) ?? false;
        var cut   = args.GetCutTimecodes(out var start, out var length);
        if (cut.Janai() && byURL.Janai())
        {
            SendManual(CUT_MANUAL);
            return;
        }

        var input = await GetFile();

        var (output, probe, options) = await input.InitEditing("Cut", Ext);

        var downloadFull = byURL && cut.Janai();
        if (downloadFull)
        {
            var size = input.FileSizeInBytes;
            if (size > 50 * 1024 * 1024)
            {
                Bot.EditMessage(Chat, MessageToEdit, GetSillyErrorMessage());
                SendBadNews(CUT_TOO_BIG_RESPONSE.Format(size.ReadableFileSize(), FAIL_EMOJI.PickAny()));
                return;
            }
        }

        var input_options = FFMpeg.InputOptions();

        if (downloadFull || start == TimeSpan.Zero && length > probe.Duration)
        {
            options.Options("-c copy");
        }
        else
        {
            var video = probe.GetPrimaryVideoStream();
            if (video != null) options.MP4_EnsureValidSize(video);

            if (start  != TimeSpan.Zero) input_options.Options($"-ss {start}");
            if (length != TimeSpan.Zero) input_options.Options($"-t {length}");

            options.Fix_AudioVideo(probe);
        }

        await FFMpeg.Args()
            .Input(input, input_options)
            .Out(output, options)
            .FFMpeg_Run();

        SendResult(output);
        Log($"{Title} >> CUT [8K-]");
    }

    protected override string VideoFileName => $"piece_fap_bot-cut-{Desert.GetSand()}.mp4";
    protected override string AudioFileName => SongNameOr($"[{Desert.GetSand()}] ((({Sender}))).mp3");
}