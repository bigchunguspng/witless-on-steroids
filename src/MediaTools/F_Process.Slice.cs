using System.Text;
using FFMpegCore;

namespace Witlesss.MediaTools;

public partial class F_Process
{
    public F_Process SliceRandom(double breaks, double pacing) => ApplyEffects(o => SliceRandomArgs(o, breaks, pacing));

    private void SliceRandomArgs(FFMpegArgumentOptions o, double breaks, double pacing)
    {
        var info = GetMediaInfo();
        var soundOnly = info is { HasAudio: true, HasVideo: false };
        var seconds = info.HasVideo ? info.Video!.Duration.TotalSeconds : info.Audio!.Duration.TotalSeconds;
        var minutes = seconds / 60;

        var timecodes = GetTrimCodes(minutes, seconds, breaks, pacing, soundOnly);

        TrimAndConcat(o, info, timecodes);

        if (info.HasVideo) o.FixPlayback();
    }

    private List<TrimCode> GetTrimCodes(double minutes, double seconds, double breaks, double pacing, bool soundOnly)
    {
        var timecodes = new List<TrimCode>();
        var head = -seconds / 2;
        while (head < seconds)
        {
            var chanceOfGoingBackwards
                = seconds <  5 ? 8
                : seconds < 30 ? 6
                : seconds < 60 ? 4 : 2;

            var direction = IsFirstOf(chanceOfGoingBackwards, 10) ? -1D : 1D;
            var step = direction
                * seconds <  5 ? RandomDouble(seconds / 20, seconds / 5)
                : seconds < 30 ? IsOneIn(3) ? RandomInt(2,  5) : seconds / 15
                : seconds < 60 ? IsOneIn(5) ? RandomInt(2, 10) : 5
                : minutes <  5 ? IsOneIn(2) ? IsOneIn(2) ? RandomInt(10, 30) : RandomInt(1, 5) :  5
                :                IsOneIn(2) ? IsOneIn(2) ? BigLeap()         : RandomInt(1, 5) : 10;

            var length = seconds < 5
                ? RandomDouble(0.15, 0.35)
                : RandomDouble(0.25, Math.Min(0.35 + 0.01 * seconds, 1.25));

            step   *= breaks; // skip: less..more
            length *= pacing; // fast..slow

            if (soundOnly && minutes < 3) length *= RandomDouble(1.5, 3);

            var a = Math.Clamp(head + step, 0, seconds);
            var b = Math.Min(a + length, seconds);

            timecodes.Add(new TrimCode(a, b));

            head = b;

            double BigLeap()
            {
                var avg = Math.Min(seconds + head, 2 * seconds - head);
                return direction * RandomInt(10, Math.Max(10, (int)(0.1 * avg)));
            }
        }

        if (timecodes[^1].B - timecodes[^1].A == 0)
            timecodes.RemoveAt(timecodes.Count - 1);

        if (seconds < 5) // SHUFFLE
        {
            var shuffles = RandomInt(0, timecodes.Count / 3);
            for (var i = 0; i < shuffles; i++)
            {
                var r1 = Random.Shared.Next(timecodes.Count);
                var r2 = Random.Shared.Next(timecodes.Count);
                (timecodes[r1], timecodes[r2]) = (timecodes[r2], timecodes[r1]);
            }
        }

        return timecodes;
    }

    private void TrimAndConcat(FFMpegArgumentOptions o, MediaInfo info, List<TrimCode> timecodes)
    {
        var count = timecodes.Count;

        for (var i = 0; i < count; i++)
        {
            var timecode = timecodes[i];
            AddInput(Input, ops => ApplyTrimming(ops, timecode));
        }

        var sb = new StringBuilder("-filter_complex \"");
        for (var i = 1; i <= count; i++)
        {
            if (info.HasVideo) sb.Append('[').Append(i).Append(":v]");
            if (info.HasAudio) sb.Append('[').Append(i).Append(":a]");
        }

        AppendConcatenation(sb, count, info).Append('"');

        o.WithCustomArgument(sb.ToString());
    }

    private void ApplyTrimming(FFMpegArgumentOptions ops, TrimCode timecode)
    {
        ops.WithCustomArgument($"-ss {Format(timecode.A)} -to {Format(timecode.B)}");
    }

    private StringBuilder AppendConcatenation(StringBuilder sb, int count, MediaInfo info)
    {
        sb.Append("concat=n=").Append(count);
        sb.Append(":v=").Append(info.HasVideo ? 1 : 0);
        sb.Append(":a=").Append(info.HasAudio ? 1 : 0);
        return sb;
    }

    private string Format(double x) => Math.Round(x, 3).Format();

    private record TrimCode(double A, double B);
}