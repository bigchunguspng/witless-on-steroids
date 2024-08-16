namespace Witlesss.Commands.Editing;

public class Load : AudioVideoPhotoCommand
{
    protected override async Task Execute()
    {
        var (path, _) = await Bot.Download(FileID, Chat);

        Bot.SendMessage(Chat, $"<code>-i {path}</code>");
        Log($"{Title} >> LOAD");
    }
}