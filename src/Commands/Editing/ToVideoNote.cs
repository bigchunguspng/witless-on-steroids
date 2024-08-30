using Telegram.Bot.Types.InputFiles;

namespace Witlesss.Commands.Editing
{
    public class ToVideoNote : VideoCommand
    {
        protected override async Task Execute()
        {
            var path = await Bot.Download(FileID, Chat, Ext);

            await using var stream = File.OpenRead(await FFMpegXD.ToVideoNote(path));
            Bot.SendVideoNote(Chat, new InputOnlineFile(stream));
            Log($"{Title} >> NOTE (*)");
        }
    }
}