using Telegram.Bot.Types;

namespace PF_Bot.Commands.Editing
{
    public class ToAnimation : VideoPhotoCommand
    {
        protected override string SyntaxManual => "/man_g";

        protected override async Task Execute()
        {
            var path = await DownloadFile();

            if (Type == MediaType.Round) path = await path.UseFFMpeg(Origin).CropVideoNoteXD();

            var photo = Type is MediaType.Photo or MediaType.Stick;
            var duration = Context.HasDoubleArgument(out var value) ? Math.Clamp(value, 0.01, 120) : 5; 
            var process = path.UseFFMpeg(Origin);
            var result = photo
                ? await process.LoopPhoto(duration).Out("-loop")
                : await process.RemoveAudio().Out("-silent");
            await using var stream = System.IO.File.OpenRead(result);
            Bot.SendAnimation(Origin, InputFile.FromStream(stream, VideoFileName));
            Log($"{Title} >> GIF [~]");
        }

        private new const string VideoFileName = "piece_fap_bot-gif.mp4";
    }
}