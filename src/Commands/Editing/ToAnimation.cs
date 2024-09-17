using Telegram.Bot.Types.InputFiles;

namespace Witlesss.Commands.Editing
{
    public class ToAnimation : VideoCommand
    {
        protected override async Task Execute()
        {
            var path = await DownloadFile();

            if (Type == MediaType.Round) path = await path.UseFFMpeg(Chat).CropVideoNoteXD();

            var result = await path.UseFFMpeg(Chat).RemoveAudio().Out("-silent");
            await using var stream = System.IO.File.OpenRead(result);
            Bot.SendAnimation(Chat, new InputOnlineFile(stream, VideoFileName));
            Log($"{Title} >> GIF [~]");
        }

        private new const string VideoFileName = "piece_fap_bot-gif.mp4";
    }
}