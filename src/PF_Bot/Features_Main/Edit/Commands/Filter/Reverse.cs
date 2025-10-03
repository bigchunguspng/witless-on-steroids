using PF_Bot.Features_Main.Edit.Core;
using PF_Tools.FFMpeg;

namespace PF_Bot.Features_Main.Edit.Commands.Filter
{
    public class Reverse : AudioVideoCommand
    {
        protected override async Task Execute()
        {
            var input = await DownloadFile();

            var (output, probe, options) = await input.InitEditing("Reverse", Ext);

            if (probe.HasVideo) options.VF( "reverse");
            if (probe.HasAudio) options.AF("areverse");

            options.Fix_AudioVideo(probe);

            await FFMpeg.Command(input, output, options).FFMpeg_Run();

            SendResult(output);
            Log($"{Title} >> REVERSED [<<]");
        }

        protected override string AudioFileName => SongNameOr($"Kid Named {Sender}.mp3");
        protected override string VideoFileName { get; } = "piece_fap_bot-reverse.mp4";
    }
}