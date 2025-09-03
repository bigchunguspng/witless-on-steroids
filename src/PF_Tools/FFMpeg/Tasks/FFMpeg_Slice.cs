using System.Text;

namespace PF_Tools.FFMpeg.Tasks;

public class FFMpeg_Slice(string input, FFProbeResult probe)
{
    private readonly FFMpegArgs _args = FFMpeg.Args();

    public FFMpegArgs ApplyRandomSlices(double breaks, double pacing)
    {
        var soundOnly = probe is { HasAudio: true, HasVideo: false };
        var seconds = probe.Duration.TotalSeconds;
        var minutes = seconds / 60;

        var timecodes = GetTrimCodes(minutes, seconds, breaks, pacing, soundOnly);

        AddInputs(timecodes);
        AddFilter(timecodes);

        return _args;
    }

    private void AddInputs(List<TrimCode> timecodes)
    {
        timecodes.ForEach(codes => _args.Input(input, $"-ss {codes.A:F3} -to {codes.B:F3}"));
    }

    private void AddFilter(List<TrimCode> timecodes)
    {
        var sb = new StringBuilder();
        var count = timecodes.Count;
        var video = probe.HasVideo;
        var audio = probe.HasAudio;
        for (var i = 0; i < count; i++)
        {
            if (video) sb.Append('[').Append(i).Append(":v]");
            if (audio) sb.Append('[').Append(i).Append(":a]");
        }

        sb.Append("concat=n=").Append(count);
        sb.Append(":v=").Append(probe.HasVideo ? 1 : 0);
        sb.Append(":a=").Append(probe.HasAudio ? 1 : 0);

        _args.Filter(sb.ToString());
    }


    // PURE LOGIC

    private record TrimCode(double A, double B);

    private static List<TrimCode> GetTrimCodes(double minutes, double seconds, double breaks, double pacing, bool soundOnly)
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
}