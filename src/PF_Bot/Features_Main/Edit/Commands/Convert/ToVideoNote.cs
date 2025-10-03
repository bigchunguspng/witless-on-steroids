using PF_Bot.Features_Main.Edit.Core;
using PF_Tools.FFMpeg;
using Telegram.Bot.Types;

namespace PF_Bot.Features_Main.Edit.Commands.Convert
{
    public class ToVideoNote : VideoCommand
    {
        protected override async Task Execute()
        {
            var input = await DownloadFile();
            var (output, probe, options) = await input.InitEditing("note", ".mp4");

            var video = probe.GetVideoStream();
            var size = video.Size;
            var diameter = Math.Min(size.Width, size.Height).ToEven();
            var x = (size.Width  - diameter) / 2;
            var y = (size.Height - diameter) / 2;

            options
                .VF($"crop={diameter}:{diameter}:{x}:{y}")
                .Resize(FFMpegOptions.VIDEONOTE_SIZE)
                .FixVideo_Playback();

            await FFMpeg.Command(input, output, options).FFMpeg_Run();

            await using var stream = System.IO.File.OpenRead(output);
            Bot.SendVideoNote(Origin, InputFile.FromStream(stream));
            Log($"{Title} >> NOTE (*)");
        }
    }
}