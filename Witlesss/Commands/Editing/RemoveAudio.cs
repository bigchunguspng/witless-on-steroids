using Telegram.Bot.Types.InputFiles;
using static Witlesss.Memes;

namespace Witlesss.Commands.Editing
{
    public class RemoveAudio : VideoCommand
    {
        public override void Run()
        {
            if (NoVideo()) return;

            Bot.Download(FileID, Chat, out string path, out var type);
            
            if (type == MediaType.Round) path = CropVideoNote(path);

            using var stream = File.OpenRead(RemoveAudio(path));
            Bot.SendAnimation(Chat, new InputOnlineFile(stream, VideoFilename()));
            Log($"{Title} >> GIF [~]");

            string VideoFilename() => "gif_fap_club.mp4";
        }
    }
}