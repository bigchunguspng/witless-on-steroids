using Telegram.Bot.Types.InputFiles;

namespace Witlesss.Commands.Editing
{
    public class ToAnimation : VideoCommand
    {
        protected override async Task Execute()
        {
            var path = await Bot.Download(FileID, Chat, Ext);

            if (Type == MediaType.Round) path = await path.UseFFMpeg(Chat).CropVideoNoteXD();

            var result = await path.UseFFMpeg(Chat).RemoveAudio().Out("-silent");
            await using var stream = File.OpenRead(result);
            Bot.SendAnimation(Chat, new InputOnlineFile(stream, VideoFileName));
            Log($"{Title} >> GIF [~]");
        }

        private new const string VideoFileName = "piece_fap_bot-gif.mp4";
    }
}