using Telegram.Bot.Types.InputFiles;
using static Witlesss.MediaTools.FFMpegXD;

namespace Witlesss.Commands.Editing
{
    public class ToAnimation : VideoCommand
    {
        protected override async Task Execute()
        {
            var path = await Bot.Download(FileID, Chat, Ext);
            
            if (Type == MediaType.Round) path = await CropVideoNote(path);

            var result = await path.UseFFMpeg().RemoveAudio().Out("-silent");
            await using var stream = File.OpenRead(result);
            Bot.SendAnimation(Chat, new InputOnlineFile(stream, VideoFileName));
            Log($"{Title} >> GIF [~]");
        }

        private new const string VideoFileName = "gif_fap_club.mp4";
    }
}