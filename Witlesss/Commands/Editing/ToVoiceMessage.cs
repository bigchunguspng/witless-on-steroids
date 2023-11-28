using Telegram.Bot.Types.InputFiles;

namespace Witlesss.Commands.Editing;

public class ToVoiceMessage : AudioVideoCommand
{
    public override void Run()
    {
        if (NothingToProcess()) return;

        Bot.Download(FileID, Chat, out var path);

        using var stream = File.OpenRead(Memes.ToVoice(path));
        Bot.SendVoice(Chat, new InputOnlineFile(stream, "balls.ogg"));
        Log($"{Title} >> VOICE ~|||~");
    }
}