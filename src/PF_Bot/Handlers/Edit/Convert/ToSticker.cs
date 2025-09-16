using PF_Bot.Core.Editing;
using PF_Tools.FFMpeg;
using Telegram.Bot.Types;

namespace PF_Bot.Handlers.Edit.Convert
{
    public class ToSticker : PhotoCommand
    {
        protected override async Task Execute()
        {
            var input = await DownloadFile();

            var (output, probe, options) = await input.InitEditing("stick", ".webp");

            var video = probe.GetVideoStream();
            var size = video.Size.Normalize();

            await FFMpeg.Command(input, output, options.Resize(size)).FFMpeg_Run();

            await using var stream = System.IO.File.OpenRead(output);
            Bot.SendSticker(Origin, InputFile.FromStream(stream));
            if (Command![^1] is 's') Bot.SendMessage(Origin, "@Stickers");
            Log($"{Title} >> STICK [!]");
        }
    }
}