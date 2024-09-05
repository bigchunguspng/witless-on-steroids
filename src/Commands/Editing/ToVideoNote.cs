using Telegram.Bot.Types.InputFiles;

namespace Witlesss.Commands.Editing
{
    public class ToVideoNote : VideoCommand
    {
        protected override async Task Execute()
        {
            var path = await Bot.Download(FileID, Chat, Ext);

            await using var stream = File.OpenRead(await path.UseFFMpeg(Chat).ToVideoNote());
            Bot.SendVideoNote(Chat, new InputOnlineFile(stream));
            Log($"{Title} >> NOTE (*)");
        }
    }
}