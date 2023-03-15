using Telegram.Bot.Types.InputFiles;

namespace Witlesss.Commands
{
    public class ToVideoNote : RemoveAudio
    {
        public override void Run()
        {
            if (NoVideo()) return;
            
            Bot.Download(FileID, Chat, out string path);
            
            using var stream = File.OpenRead(Bot.MemeService.ToVideoNote(path));
            Bot.SendVideoNote(Chat, new InputOnlineFile(stream));
            Log($"{Title} >> NOTE (*)");
        }

        protected override int VideoNoteSize() => 384;
    }
}