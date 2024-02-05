using Telegram.Bot.Types;

namespace Witlesss.Commands.Editing;

#pragma warning disable CS4014

public class Slice : VideoCommand
{
    protected override void Execute()
    {
        Bot.RunSafelyAsync(new SliceAsync(SnapshotMessageData(), FileID).RunAsync(), Chat, -1);
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
}