using PF_Bot.Features_Main.Edit.Core;
using PF_Bot.Routing.Commands;
using PF_Tools.FFMpeg;

namespace PF_Bot.Features_Main.Edit.Commands.Convert
{
    public class ToSticker : FileEditor_Photo
    {
        protected override async Task Execute()
        {
            var input = await GetFile();

            var (output, probe, options) = await input.InitEditing("stick", ".webp");

            var video = probe.GetVideoStream();
            var size = video.Size.Normalize(512);

            await FFMpeg.Command(input, output, options.Resize(size)).FFMpeg_Run();

            SendFile(output, MediaType.Stick);
            if (Options.EndsWith('s')) Bot.SendMessage(Origin, "@Stickers");
            Log($"{Title} >> STICK [!]");
        }
    }
}