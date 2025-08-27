using PF_Bot.Features.Edit.Core;
using Telegram.Bot.Types;
using static PF_Bot.Tools_Legacy.FFMpeg.FFMpegXD;

namespace PF_Bot.Features.Edit.Convert
{
    public class ToSticker : PhotoCommand
    {
        protected override async Task Execute()
        {
            var path = await DownloadFile();

            var size = GetPictureSize(path).Normalize().Ok();
            var result = await path.UseFFMpeg(Origin).ToSticker(size).Out("-stick", ".webp");
            await using var stream = System.IO.File.OpenRead(result);
            Bot.SendSticker(Origin, InputFile.FromStream(stream));
            if (Command![^1] is 's') Bot.SendMessage(Origin, "@Stickers");
            Log($"{Title} >> STICK [!]");
        }
    }
}