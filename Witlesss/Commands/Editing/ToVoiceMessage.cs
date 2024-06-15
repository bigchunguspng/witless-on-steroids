using System.Threading.Tasks;
using Telegram.Bot.Types.InputFiles;

namespace Witlesss.Commands.Editing;

public class ToVoiceMessage : FileEditingCommand
{
    protected override async Task Execute()
    {
        var (path, _) = await Bot.Download(FileID, Chat);

        string result;
        try
        {
            result = await Memes.ToVoice(path);
        }
        catch
        {
            result = Paths.File_DefaultVoiceMessage;
        }

        await using var stream = File.OpenRead(result);
        Bot.SendVoice(Chat, new InputOnlineFile(stream, "balls.ogg"));
        Log($"{Title} >> VOICE ~|||~");
    }
}