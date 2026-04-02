using PF_Tools.FFMpeg;
using static PF_Tools.Backrooms.Helpers.Fortune;

namespace PF_Bot.Features_Main.Edit.Core;

public record struct RandomizeOptions
(
    (int from, int to) Range_Repeats,
    (int from, int to) Range_Nuke,
    byte Percent_Repeats,
    byte Percent_Nuke,
    byte Percent_Sfx,  // volume, eq
    byte Percent_Time, // fast / slow, pitch
    byte Percent_Crop, // pan, scale, transform etc
    bool sorted
);

public partial class FFMpeg_Effects
{
    public FFMpegArgs FX_Random
        (double piece_len_mul, double break_len_mul, RandomizeOptions options, TimeSelection selection = default)
    {
        var soundOnly = SoundOnly(); // ↓ default: 0.1, 0.25
        var timecodes = GetTrimCodes(piece_len_mul, break_len_mul, soundOnly, selection, go_back_chance_mul: 200);
        var fragments = GenerateSchematic(timecodes, options, soundOnly);

        // todo random-specific slicing code
        // repeats - not equal in length, may be offset
        // slice jump back and forth with higher chance

        AddInputs(fragments);
        AddFilter(fragments, soundOnly);

        return _args;
    }

    private record Fragment(TrimCode Trim, int Copy) // repeat number, 0 - first occurence
    {
        public int    Nuke  { get; set; }
        public double Speed { get; set; } = 1.0;

        public bool TimeStretch => Math.Abs(1.0 - Speed) > 0.005;

        public bool UseOriginalVideo => TimeStretch.IsOff() && Nuke == 0;
        public bool UseOriginalAudio => TimeStretch.IsOff();
    }

    // PHASE I

    private List<Fragment> GenerateSchematic
        (List<TrimCode> timecodes, RandomizeOptions options, bool soundOnly)
    {
        var fragments = new List<Fragment>(timecodes.Count);

        var video = probe.HasVideo && soundOnly.Janai();
        var audio = probe.HasAudio;

        // MAP + REPEATS
        var adjust_timecodes = IsOneIn(2);
        foreach (var timecode in timecodes)
        {
            fragments.Add(new Fragment(timecode, 0));
            var repeat = LuckyFor(options.Percent_Repeats);
            if (repeat)
            {
                var (from, to) = options.Range_Repeats;
                var number = RandomInt(from, to);
                for (var i = 0; i < number; i++)
                {
                    if (LuckyFor(adjust_timecodes ? 20 : 35)) adjust_timecodes = !adjust_timecodes;

                    var length = timecode.Length;
                    var start  = timecode.Start + (adjust_timecodes ? RandomDouble(-0.25 * length, 0.25 * length) : 0);
                    var end    = timecode.End   + (adjust_timecodes ? RandomDouble(-0.25 * length, 0.25 * length) : 0);
                    fragments.Add(new Fragment(new TrimCode(start, end), i + 1));
                }
            }
        }

        // FX
        foreach (var fragment in fragments)
        {
            if (video)
            {
                var nuke = LuckyFor(options.Percent_Nuke);
                if (nuke)
                {
                    var (from, to) = options.Range_Nuke;
                    fragment.Nuke = RandomInt(from, to);
                }
            }

            if (audio)
            {

            }

            // (any)
            {
                var time = LuckyFor(options.Percent_Time);
                if (time)
                {
                    fragment.Speed = IsOneIn(2)
                        ? RandomDouble(0.5, 1.0)
                        : RandomDouble(1.0, 2.0);
                    // todo chance of very slow frag
                }
            }
        }

        return fragments;
    }

    // PHASE II

    private void AddInputs(List<Fragment> fragments)
    {
        fragments.ForEach(frag => _args.Input(input, $"-ss {frag.Trim.Start:F3} -to {frag.Trim.End:F3}"));
    }

    private void AddFilter(List<Fragment> fragments, bool soundOnly)
    {
        var count = fragments.Count;
        var video = probe.HasVideo && soundOnly.Janai();
        var audio = probe.HasAudio;

        // FX
        for (var i = 0; i < count; i++)
        {
            var frag = fragments[i];

            if (video && frag.UseOriginalVideo.Janai()) // VFX
            {
                _args.Filter($"[{i}:v]");

                if (frag.TimeStretch)
                {
                    _args.FilterAppend($"setpts={1 / frag.Speed:F3}*PTS");
                    _args.FilterAppend($"fps={probe.GetVideoStream().AvgFramerate}");
                }

                if (frag.Nuke > 0)
                {
                    for (var j = 0; j < frag.Nuke; j++)
                    {
                        DropNuke(isVideo: true);
                    }
                }

                _args.FilterAppend($"[v{i}]");
            }

            if (audio && frag.UseOriginalAudio.Janai()) // AFX
            {
                _args.Filter($"[{i}:a]");

                if (frag.TimeStretch)
                {
                    _ = IsOneIn(2)
                        ? _args.FilterAppend  ($"atempo={frag.Speed:F3}") // maintain pitch
                        : _args.FilterAppend($"asetrate={frag.Speed:F3}*{probe.GetAudioStream().SampleRate}"); // change pitch
                }

                _args.FilterAppend($"[a{i}]");
            }
        }

        _args.FilterAppend(";");

        // CONCAT
        for (var i = 0; i < count; i++)
        {
            var frag = fragments[i];

            if (video)
                _ = frag.UseOriginalVideo
                    ? _args.FilterAppend($"[{i}:v]")
                    : _args.FilterAppend($"[v{i}]");
            if (audio)
                _ = frag.UseOriginalAudio
                    ? _args.FilterAppend($"[{i}:a]")
                    : _args.FilterAppend($"[a{i}]");
        }

        _args.FilterAppend($"concat=n={count}:v={(video ? 1 : 0)}:a={(audio ? 1 : 0)}");
    }
}