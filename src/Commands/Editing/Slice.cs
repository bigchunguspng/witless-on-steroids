namespace Witlesss.Commands.Editing;

public class Slice : AudioVideoUrlCommand
{
    private static readonly Regex _multiplier = new(@"\d+");

    protected override async Task Execute()
    {
        var (path, waitMessage) = await DownloadFileSuperCool();

        var multiplier = Math.Max(1, _multiplier.ExtractGroup(0, Command!, int.Parse, 5)) / 5D;

        var sw = GetStartedStopwatch();

        if (Type != MediaType.Audio) path = await FFMpegXD.ReduceSize(Origin, path);

        var result = await path.UseFFMpeg(Origin).SliceRandom(multiplier).Out("-slices", Ext);

        Bot.DeleteMessageAsync(Chat, waitMessage);

        SendResult(result);
        Log($"{Title} >> SLICED [~/~] >> {sw.ElapsedShort()}");
    }

    protected override string AudioFileName { get; } = "sliced_by_piece_fap_bot.mp3";
    protected override string VideoFileName { get; } = "piece_fap_slice.mp4";
}