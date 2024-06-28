using System.Threading.Tasks;
using Telegram.Bot.Types.InputFiles;
using Witlesss.MediaTools;

namespace Witlesss.Commands.Editing
{
    public class ToVideoNote : VideoCommand
    {
        protected override async Task Execute()
        {
            var (path, _) = await Bot.Download(FileID, Chat);

            await using var stream = File.OpenRead(await FFMpegXD.ToVideoNote(path));
            Bot.SendVideoNote(Chat, new InputOnlineFile(stream));
            Log($"{Title} >> NOTE (*)");
        }
    }
}