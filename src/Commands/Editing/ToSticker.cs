using Telegram.Bot.Types.InputFiles;
using static Witlesss.MediaTools.FFMpegXD;

namespace Witlesss.Commands.Editing
{
    public class ToSticker : PhotoCommand
    {
        protected override async Task Execute()
        {
            var (path, _) = await Bot.Download(FileID, Chat);

            await using var stream = File.OpenRead(await Stickerize(path));
            Bot.SendSticker(Chat, new InputOnlineFile(stream));
            if (Command![^1] is 's') Bot.SendMessage(Chat, "@Stickers");
            Log($"{Title} >> STICK [!]");
        }

        protected override bool ChatIsBanned() => false;
    }
}