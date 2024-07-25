using System.Threading.Tasks;
using Telegram.Bot.Types.InputFiles;
using static Witlesss.MediaTools.FFMpegXD;

namespace Witlesss.Commands.Editing
{
    public class ToAnimation : VideoCommand
    {
        protected override async Task Execute()
        {
            var (path, type) = await Bot.Download(FileID, Chat);
            
            if (type == MediaType.Round) path = await CropVideoNote(path);

            await using var stream = File.OpenRead(await RemoveAudio(path));
            Bot.SendAnimation(Chat, new InputOnlineFile(stream, VideoFileName));
            Log($"{Title} >> GIF [~]");
        }

        private new const string VideoFileName = "gif_fap_club.mp4";
    }
}