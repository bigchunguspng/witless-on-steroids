using Telegram.Bot.Types.InputFiles;

namespace Witlesss.Commands.Editing
{
    public class ToVideoNote : VideoCommand
    {
        public override void Run()
        {
            if (NothingToProcess()) return;
            
            Bot.Download(FileID, Chat, out var path);
            
            using var stream = File.OpenRead(Memes.ToVideoNote(path));
            Bot.SendVideoNote(Chat, new InputOnlineFile(stream));
            Log($"{Title} >> NOTE (*)");
        }
    }
}