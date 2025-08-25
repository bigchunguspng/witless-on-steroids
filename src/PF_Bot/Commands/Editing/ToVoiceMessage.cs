using Telegram.Bot.Types;

namespace PF_Bot.Commands.Editing;

public class ToVoiceMessage : AudioVideoCommand
{
    protected override async Task Execute()
    {
        var path = await DownloadFile();

        string result;
        try
        {
            result = await path.UseFFMpeg(Origin).ToVoice().Out("-voice", ".ogg");
        }
        catch
        {
            result = File_DefaultVoiceMessage;
        }

        await using var stream = System.IO.File.OpenRead(result);
        Bot.SendVoice(Origin, InputFile.FromStream(stream, "balls.ogg"));
        Log($"{Title} >> VOICE ~|||~");
    }
}