namespace PF_Bot.Features_Main.Edit.Commands.Manual;

public class Load : FileEditor_AudioVideoPhoto
{
    protected override async Task Execute()
    {
        var path = await GetFile();

        Bot.SendMessage(Origin, $"<code>{path}</code>");
        Log($"{Title} >> LOAD");
    }
}