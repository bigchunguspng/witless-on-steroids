using Telegram.Bot.Types.InputFiles;
using static Witlesss.MediaTools.FFMpegXD;

namespace Witlesss.Commands.Editing
{
    public class ToSticker : PhotoCommand
    {
        protected override async Task Execute()
        {
            var path = await Bot.Download(FileID, Chat, Ext);

            var size = GetPictureSize(path).Normalize().Ok();
            var result = await path.UseFFMpeg().ToSticker(size).Out("-stick", ".webp");
            await using var stream = File.OpenRead(result);
            Bot.SendSticker(Chat, new InputOnlineFile(stream));
            if (Command![^1] is 's') Bot.SendMessage(Chat, "@Stickers");
            Log($"{Title} >> STICK [!]");
        }

        protected override bool ChatIsBanned() => false;
    }
}