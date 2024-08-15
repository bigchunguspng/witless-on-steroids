using Telegram.Bot.Types.InputFiles;

namespace Witlesss.Commands.Editing;

public class ToVoiceMessage : AudioVideoCommand
{
    protected override async Task Execute()
    {
        var (path, _) = await Bot.Download(FileID, Chat);

        string result;
        try
        {
            result = await FFMpegXD.ToVoice(path);
        }
        catch
        {
            result = File_DefaultVoiceMessage;
        }

        await using var stream = File.OpenRead(result);
        Bot.SendVoice(Chat, new InputOnlineFile(stream, "balls.ogg"));
        Log($"{Title} >> VOICE ~|||~");
    }
}