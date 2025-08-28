using PF_Bot.Features.Edit.Core;
using PF_Bot.Features.Edit.Shared;
using PF_Tools.FFMpeg;
using Telegram.Bot.Types;

namespace PF_Bot.Features.Edit.Convert
{
    public class ToSticker : PhotoCommand
    {
        protected override async Task Execute()
        {
            var input = await DownloadFile();
            var output = EditingHelpers.GetOutputFilePath(input, "stick", ".webp");

            var video = await EditingHelpers.GetVideoStream(input);
            var size = video.Size.Ok().Normalize().Ok();
            var args = FFMpeg.Args().Input(input).Out(output, o => o.Resize(size.Ok()));
            await EditingHelpers.FFMpeg_Run(args);

            await using var stream = System.IO.File.OpenRead(output);
            Bot.SendSticker(Origin, InputFile.FromStream(stream));
            if (Command![^1] is 's') Bot.SendMessage(Origin, "@Stickers");
            Log($"{Title} >> STICK [!]");
        }
    }
}