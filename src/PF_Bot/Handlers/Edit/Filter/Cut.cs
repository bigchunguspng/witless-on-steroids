using PF_Bot.Backrooms.Helpers;
using PF_Bot.Handlers.Edit.Core;
using PF_Bot.Handlers.Edit.Shared;
using PF_Tools.FFMpeg;

namespace PF_Bot.Handlers.Edit.Filter
{
    public class Cut : AudioVideoUrlCommand
    {
        protected override string SyntaxManual => "/man_cut";

        protected override async Task Execute()
        {
            var args = Args?.Split().SkipWhile(x => x.StartsWith('/') || x.StartsWith("http")).ToArray();

            var parsing = ArgumentParsing.GetCutTimecodes(args);
            if (parsing.Failed)
            {
                Bot.SendMessage(Origin, CUT_MANUAL);
                return;
            }

            var (_, start, length) = parsing;

            var (input, waitMessage) = await DownloadFileSuperCool();
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

            Bot.DeleteMessageAsync(Chat, waitMessage);

            SendResult(output);
            Log($"{Title} >> CUT [8K-]");
        }

        protected override string VideoFileName => "piece_fap_bot-cut.mp4";
        protected override string AudioFileName => SongNameOr($"((({Sender}))).mp3");
    }
}