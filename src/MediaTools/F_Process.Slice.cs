using System.Text;
using FFMpegCore;

namespace Witlesss.MediaTools;

public partial class F_Process
{
    public F_Process SliceRandom() => ApplyEffects(SliceRandomArgs);

    private void SliceRandomArgs(FFMpegArgumentOptions o)
    {
        var info = GetMediaInfo();
        var soundOnly = info is { HasAudio: true, HasVideo: false };
        var seconds = info.HasVideo ? info.Video.Duration.TotalSeconds : info.Audio.Duration.TotalSeconds;
        var minutes = seconds / 60;

        var timecodes = new List<TrimCode>();
        var head = -seconds / 2;
        while (head < seconds)
        {
            var step = IsFirstOf(seconds < 5 ? 8 : seconds < 30 ? 6 : seconds < 60 ? 4 : 2, 10) ? -1d : 1d;
            step *=
                  seconds <  5 ? RandomDouble(seconds / 20, seconds / 10)
                : seconds < 30 ? IsOneIn(3) ? RandomInt(2,  5) : seconds / 15
                : seconds < 60 ? IsOneIn(5) ? RandomInt(2, 10) : 5
                : minutes <  5 ? IsOneIn(2) ? IsOneIn(2) ? RandomInt(10, 30) : RandomInt(1, 5) :  5
                :                IsOneIn(2) ? IsOneIn(2) ? BigLeap()         : RandomInt(1, 5) : 10;

            var length = seconds < 5
                ? RandomDouble(0.15, 0.35)
                : RandomDouble(0.25, Math.Min(0.35 + 0.01 * seconds, 1.25));

            if (soundOnly && minutes < 3) length *= RandomDouble(1.5, 3);

            var a = Math.Clamp(head + step, 0, seconds - 0.15);
            var b = Math.Min(a + length, seconds);

            timecodes.Add(new TrimCode(a, b));

            head = b;

            double BigLeap()
            {
                var avg = Math.Min(seconds + head, 2 * seconds - head);
                return step * RandomInt(10, Math.Max(10, (int)(0.1 * avg)));
            }
        }

        var onePiece = soundOnly || minutes <= 2 || minutes <= 5 && timecodes.Count <= 24;
        if (onePiece) ApplyTrims(o, info, timecodes);
        else    TrimPieceByPiece(o, info, timecodes, seconds);

        if (info.HasVideo) o.FixPlayback();
    }

    private void TrimPieceByPiece(FFMpegArgumentOptions o, MediaInfo info, List<TrimCode> timecodes, double seconds)
    {
        var count = seconds > 300 ? (int)Math.Ceiling(seconds / 210) : 2;
        var parts = new string[count];
        var codes = new double[count];
        var takes = new int   [count];

        Log($"SLICING LONG VIDEO!!! ({count} parts, {timecodes.Count} trims)", ConsoleColor.Yellow);

        var pieceLength = seconds / count;

        var head = -1;
        for (var i = 0; i < count; i++) // split video into chunks
        {
            var start = i * pieceLength;
            var tail = timecodes.FindLastIndex(x => x.A > start && x.B <= start + pieceLength);
            if (tail < 0) continue;

            var window = timecodes.Take(tail + 1).Skip(head + 1).ToList();
            var ss = Math.Max(window.Min(x => x.A) - 10, 0);
            var to = Math.Min(window.Max(x => x.B) + 5, seconds);

            head = tail;

            var index = i;

            codes[i] = ss;
            takes[i] = window.Count;
            parts[i] = new F_Process(Input, Chat).ApplyEffects(ops =>
            {
                var builder = new StringBuilder("-c copy ");
                if (index > 0) builder.Append("-ss ").Append(Format(ss)).Append(' ');
                if (index + 1 < count) builder.Append("-to ").Append(Format(to));

                ops.WithCustomArgument(builder.ToString());
            }).Out($"-part-{i}", Path.GetExtension(Input)).Result;
        }

        var slices = new List<string>();

        for (var i = 0; i < count; i++) // slice each chunk
        {
            if (!File.Exists(parts[i]))
            {
                Log($"PART {i + 1} >> SKIP", ConsoleColor.Yellow);
                continue;
            }

            var take = takes[i];
            var offset = takes.Take(i).Sum();
            var start = codes[i];
            slices.Add(new F_Process(parts[i], Chat).ApplyEffects(ops =>
            {
                ApplyTrims(ops, info, timecodes, offset, take, start);
            }).Out("-slices", Path.GetExtension(Input)).Result);

            Log($"PART {i + 1} >> DONE", ConsoleColor.Yellow);
        }

        var sb = new StringBuilder(); // combine the results
        foreach (var slice in slices) sb.Append("-i \"").Append(slice).Append("\" ");

        count = slices.Count;

        sb.Append("-filter_complex \"");
        for (var i = 1; i <= count; i++)
        {
            if (info.HasVideo) sb.Append('[').Append(i).Append(":v]");
            if (info.HasAudio) sb.Append('[').Append(i).Append(":a]");
        }

        AppendConcatenation(sb, count, info).Append('"');

        o.WithCustomArgument(sb.ToString());
    }

    private void ApplyTrims
    (
        FFMpegArgumentOptions o, MediaInfo info, List<TrimCode> timecodes, int offset = 0, int take = 0,
        double start = 0
    )
    {
        var count = take == 0 ? timecodes.Count : Math.Min(take, timecodes.Count - offset);

        var sb = new StringBuilder("-filter_complex \"");
        for (var i = offset; i < offset + count; i++)
        {
            if (info.HasVideo) AppendTrimming('v', "");
            if (info.HasAudio) AppendTrimming('a', "a");

            void AppendTrimming(char av, string a)
            {
                sb.Append("[0:").Append(av).Append(']').Append(a).Append("trim=");
                sb.Append(Format(timecodes[i].A - start)).Append(':');
                sb.Append(Format(timecodes[i].B - start));
                sb.Append(',').Append(a).Append("setpts=PTS-STARTPTS[").Append(av).Append(i).Append("];");
            }
        }

        for (var i = offset; i < offset + count; i++)
        {
            if (info.HasVideo) sb.Append("[v").Append(i).Append(']');
            if (info.HasAudio) sb.Append("[a").Append(i).Append(']');
        }

        AppendConcatenation(sb, count, info).Append('"');

        o.WithCustomArgument(sb.ToString());
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