using Telegram.Bot.Types;

namespace PF_Bot.Commands.Editing
{
    public class ToVideoNote : VideoCommand
    {
        protected override async Task Execute()
        {
            var path = await DownloadFile();

            await using var stream = System.IO.File.OpenRead(await path.UseFFMpeg(Origin).ToVideoNote());
            Bot.SendVideoNote(Origin, InputFile.FromStream(stream));
            Log($"{Title} >> NOTE (*)");
        }
    }
}