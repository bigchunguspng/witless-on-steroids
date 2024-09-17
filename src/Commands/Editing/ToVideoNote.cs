using Telegram.Bot.Types.InputFiles;

namespace Witlesss.Commands.Editing
{
    public class ToVideoNote : VideoCommand
    {
        protected override async Task Execute()
        {
            var path = await Bot.Download(File, Chat, Ext);

            await using var stream = System.IO.File.OpenRead(await path.UseFFMpeg(Chat).ToVideoNote());
            Bot.SendVideoNote(Chat, new InputOnlineFile(stream));
            Log($"{Title} >> NOTE (*)");
        }
    }
}