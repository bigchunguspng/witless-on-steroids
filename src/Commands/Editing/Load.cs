namespace Witlesss.Commands.Editing;

public class Load : AudioVideoPhotoCommand
{
    protected override async Task Execute()
    {
        var path = await Bot.Download(File, Chat, Ext);

        Bot.SendMessage(Chat, $"<code>-i {path}</code>");
        Log($"{Title} >> LOAD");
    }
}