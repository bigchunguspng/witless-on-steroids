namespace Witlesss.Commands.Editing;

public class Load : AudioVideoPhotoCommand
{
    protected override async Task Execute()
    {
        var path = await Bot.Download(FileID, Chat, Ext);

        Bot.SendMessage(Chat, $"<code>-i {path}</code>");
        Log($"{Title} >> LOAD");
    }
}