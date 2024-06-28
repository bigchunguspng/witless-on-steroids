using System.Threading.Tasks;
using Telegram.Bot.Types;
using Witlesss.MediaTools;

namespace Witlesss.Commands.Editing;

#pragma warning disable CS4014

public class Slice : VideoCommand
{
    protected override async Task Execute()
    {
        var (path, type, waitMessage) = await DownloadFileSuperCool();

        var result = await FFMpegXD.Slice(path);

        Task.Run(() => Bot.DeleteMessage(Chat, waitMessage));

        SendResult(result, type);
        Log($"{Title} >> SLICED [~/~]");
    }

    protected override string Manual { get; } = SLICE_MANUAL;

    protected override bool MessageContainsFile(Message m)
    {
        return GetVideoFileID(m) || GetAudioFileID(m) || GetVideoURL(m);
    }

    private bool GetVideoURL(Message m)
    {
        if (m.Text is null) return false;

        var s = m.Text.Split();
        if                      (s[0].StartsWith("http")) FileID = s[0];
        else if (s.Length > 1 && s[1].StartsWith("http")) FileID = s[1];
        else return false;

        return true;
    }

    protected override string AudioFileName { get; } = "sliced_by_piece_fap_bot.mp3";
    protected override string VideoFileName { get; } = "piece_fap_slice.mp4";
}