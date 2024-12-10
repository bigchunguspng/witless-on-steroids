namespace Witlesss.Commands.Editing;

public class Load : AudioVideoPhotoCommand
{
    protected override async Task Execute()
    {
        var path = await DownloadFile();

        Bot.SendMessage(Origin, $"<code>-i {path}</code>");
        Log($"{Title} >> LOAD");
    }
}