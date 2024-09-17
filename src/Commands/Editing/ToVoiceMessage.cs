using Telegram.Bot.Types.InputFiles;

namespace Witlesss.Commands.Editing;

public class ToVoiceMessage : AudioVideoCommand
{
    protected override async Task Execute()
    {
        var path = await DownloadFile();

        string result;
        try
        {
            result = await path.UseFFMpeg(Chat).ToVoice().Out("-voice", ".ogg");
        }
        catch
        {
            result = File_DefaultVoiceMessage;
        }

        await using var stream = System.IO.File.OpenRead(result);
        Bot.SendVoice(Chat, new InputOnlineFile(stream, "balls.ogg"));
        Log($"{Title} >> VOICE ~|||~");
    }
}