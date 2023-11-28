using Telegram.Bot.Types.InputFiles;

namespace Witlesss.Commands.Editing;

public class ToVoiceMessage : AudioVideoCommand
{
    public override void Run()
    {
        if (NothingToProcess()) return;

        Bot.Download(FileID, Chat, out var path);

        string voice;
        try
        {
            voice = Memes.ToVoice(path);
        }
        catch
        {
            voice = "voice.ogg";
        }

        using var stream = File.OpenRead(voice);
        Bot.SendVoice(Chat, new InputOnlineFile(stream, "balls.ogg"));
        Log($"{Title} >> VOICE ~|||~");
    }
}