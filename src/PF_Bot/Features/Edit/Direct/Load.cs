using PF_Bot.Features.Edit.Core;

namespace PF_Bot.Features.Edit.Direct;

public class Load : AudioVideoPhotoCommand
{
    protected override async Task Execute()
    {
        var path = await DownloadFile();

        Bot.SendMessage(Origin, $"<code>{path}</code>");
        Log($"{Title} >> LOAD");
    }
}