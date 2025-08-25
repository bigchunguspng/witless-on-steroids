namespace PF_Bot.Commands.Editing;

public class Load : AudioVideoPhotoCommand
{
    protected override async Task Execute()
    {
        var path = await DownloadFile();

        Bot.SendMessage(Origin, $"<code>{path}</code>");
        Log($"{Title} >> LOAD");
    }
}