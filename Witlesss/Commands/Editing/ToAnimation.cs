using Telegram.Bot.Types.InputFiles;
using static Witlesss.Memes;

namespace Witlesss.Commands.Editing
{
    public class ToAnimation : VideoCommand
    {
        protected override void Execute()
        {
            Bot.Download(FileID, Chat, out var path, out var type);
            
            if (type == MediaType.Round) path = CropVideoNote(path);

            using var stream = File.OpenRead(RemoveAudio(path));
            Bot.SendAnimation(Chat, new InputOnlineFile(stream, VideoFileName));
            Log($"{Title} >> GIF [~]");
        }

        private new const string VideoFileName = "gif_fap_club.mp4";
    }
}