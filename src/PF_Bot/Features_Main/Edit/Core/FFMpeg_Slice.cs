using System.Text;
using PF_Tools.FFMpeg;
using static PF_Tools.Backrooms.Helpers.Fortune;

namespace PF_Bot.Features_Main.Edit.Core;

public readonly record struct TimeSelection(TimeSpan Start, TimeSpan Length)
{
    public TimeSpan End => Start + Length;
}

public partial class FFMpeg_Effects(string input, FFProbeResult probe)
{
    private readonly FFMpegArgs _args = FFMpeg.Args();

    private bool SoundOnly
        () => probe.HasAudio && (probe.HasVideo.Janai() || probe.GetVideoStream().IsLikelyImage);
}

public partial class FFMpeg_Effects
{
    public FFMpegArgs FX_Slice
        (double piece_len_mul, double break_len_mul, TimeSelection selection = default)
    {
        var soundOnly = SoundOnly();
        var timecodes = GetTrimCodes(piece_len_mul, break_len_mul, soundOnly, selection);

        AddInputs(timecodes);
        AddFilter(timecodes, soundOnly);

        return _args;
    }

    private void AddInputs(List<TrimCode> timecodes)
    {
        timecodes.ForEach(trim => _args.Input(input, $"-ss {trim.Start:F3} -to {trim.End:F3}"));
    }

    private void AddFilter(List<TrimCode> timecodes, bool soundOnly)
    {
        var sb = new StringBuilder();
        var count = timecodes.Count;
        var video = probe.HasVideo && soundOnly.Janai();
        var audio = probe.HasAudio;
        for (var i = 0; i < count; i++)
        {
            if (video) sb.Append('[').Append(i).Append(":v]");
            if (audio) sb.Append('[').Append(i).Append(":a]");
        }

        sb.Append("concat=n=").Append(count);
        sb.Append(":v=").Append(video ? 1 : 0);
        sb.Append(":a=").Append(audio ? 1 : 0);

        _args.Filter(sb.ToString());
    }

    // PURE LOGIC

    protected readonly record struct TrimCode(double Start, double End)
    {
        public double Length => End - Start;
    }

    private List<TrimCode> GetTrimCodes
        (double piece_len_mul, double break_len_mul, bool soundOnly, TimeSelection selection, int go_back_chance_mul = 750)
    {
        var offset   = selection.Start.TotalSeconds;
        var duration = selection.Length == TimeSpan.Zero
            ?                             probe.Duration  - selection.Start
            : TimeMath.Min(selection.End, probe.Duration) - selection.Start;

        var seconds = duration.TotalSeconds;
        var minutes = seconds / 60;

        var timecodes = new List<TrimCode>();
        var head = -seconds / 2;
        while (head < seconds) // PICK A TIMECODE
        {
            var goBack_pc = Math.Clamp(10 + go_back_chance_mul / (seconds + 15), 10, 80).RoundInt();
            var direction = LuckyFor(goBack_pc) ? -1.0 : 1.0;

            //                      ↓  1m - 0%,  5m - 10%,  60m - 25%,  3h - 33%
            var step_abs = LuckyFor((15 * Math.Log10(minutes)).RoundInt())
                ? BigLeap()
                : LuckyFor(20)
                    ? RandomDouble(Math.Min(1, seconds / 300), seconds / 20) // short  leap
                    : RandomDouble(seconds / 20, seconds / 5);               // medium leap (main case)

            var step = direction * Math.Max(0.025, step_abs);

            var length = seconds < 10
                ? RandomDouble(0.15 + 0.01 * seconds, 0.35 + 0.01 * seconds) // 0.15..0.35 - 0.25..0.45
                : RandomDouble(0.25,   Math.Min(1.25, 0.35 + 0.01 * seconds));            // 0.25..0.45  -90s-  0.25..1.25

            step   *= break_len_mul;
            length *= piece_len_mul;

            if (soundOnly && minutes < 3) length *= RandomDouble(1.5, 3);

            var a = Math.Clamp(head + step, 0, seconds);
            var b = Math.Min(a + length, seconds);

            timecodes.Add(new TrimCode(offset + a, offset + b));

            head = b;

            double BigLeap() // seconds >= 60
            {
                var edge_distance = Math.Min(head, seconds - head);
                var s = (seconds + edge_distance) / 10; // 60s - 6-9s,  5m - 30-45s
                var abs = Math.Abs(10 - s);
                var extra = abs < 5
                    ? 0.5 * (5 - abs)
                    : 0;
                var min = Math.Min(10, s) - extra;
                var max = Math.Max(10, s) + extra;
                return direction * RandomDouble(min, max); // only forward!
            }
        }

        if (timecodes.Count > 0 && timecodes[^1].Length == 0)
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