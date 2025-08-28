using PF_Bot.Features.Edit.Core;
using PF_Bot.Features.Edit.Shared;
using PF_Tools.FFMpeg;
using Telegram.Bot.Types;

namespace PF_Bot.Features.Edit.Convert
{
    public class ToVideoNote : VideoCommand
    {
        protected override async Task Execute()
        {
            var input = await DownloadFile();
            var output = EditingHelpers.GetOutputFilePath(input, "vn", ".mp4");

            var video = await EditingHelpers.GetVideoStream(input);
            var size = video.Size;
            var diameter = Math.Min(size.Width, size.Height).ToEven();
            var x = (size.Width  - diameter) / 2;
            var y = (size.Height - diameter) / 2;

            var options = FFMpeg.OutputOptions()
                .VF($"crop={diameter}:{diameter}:{x}:{y}")
                .Resize(FFMpegOptions.VIDEONOTE_SIZE).FixVideoPlayback();

            var args = FFMpeg.Args().Input(input).Out(output, options);
            await EditingHelpers.FFMpeg_Run(args);

            await using var stream = System.IO.File.OpenRead(output);
            Bot.SendVideoNote(Origin, InputFile.FromStream(stream));
            Log($"{Title} >> NOTE (*)");
        }
    }
}