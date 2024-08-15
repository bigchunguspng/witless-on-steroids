namespace Witlesss.Commands.Editing;

public class Slice : AudioVideoUrlCommand
{
    protected override async Task Execute()
    {
        var (path, type, waitMessage) = await DownloadFileSuperCool();

        var result = await FFMpegXD.Slice(path);

        Bot.DeleteMessageAsync(Chat, waitMessage);

        SendResult(result, type);
        Log($"{Title} >> SLICED [~/~]");
    }

    protected override string AudioFileName { get; } = "sliced_by_piece_fap_bot.mp3";
    protected override string VideoFileName { get; } = "piece_fap_slice.mp4";
}