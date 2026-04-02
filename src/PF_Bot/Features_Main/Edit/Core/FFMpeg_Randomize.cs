using PF_Tools.FFMpeg;
using static PF_Tools.Backrooms.Helpers.Fortune;

namespace PF_Bot.Features_Main.Edit.Core;

public record struct RandomizeOptions
(
    (int from, int to) Range_Repeats,
    (int from, int to) Range_Nuke,
    byte Percent_Repeats,
    byte Percent_Nuke,
    byte Percent_SFX,  // volume, eq
    byte Percent_Time, // fast / slow, pitch
    byte Percent_Crop, // pan, scale, transform etc
    bool Ordered
);

public partial class FFMpeg_Effects
{
    public FFMpegArgs FX_Random
        (double piece_len_mul, double break_len_mul, RandomizeOptions options, TimeSelection selection = default)
    {
        var soundOnly = SoundOnly(); // ↓ default: 0.1, 0.25
        var timecodes = GetTrimCodes(piece_len_mul, break_len_mul, soundOnly, selection, 200, options.Ordered.IsOff());
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
        public bool   SFX   { get; set; }
        public int    Nuke  { get; set; }
        public double Speed { get; set; } = 1.0;

        public bool TimeStretch => Math.Abs(1.0 - Speed) > 0.005;

        public bool UseOriginalVideo => TimeStretch.IsOff() && Nuke == 0;
        public bool UseOriginalAudio => TimeStretch.IsOff() && SFX.IsOff();
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
                if (LuckyFor(options.Percent_Nuke))
                {
                    var (from, to) = options.Range_Nuke;
                    fragment.Nuke = RandomInt(from, to);
                }
            }

            if (audio)
            {
                fragment.SFX = LuckyFor(options.Percent_SFX);
            }

            // (any)
            {
                if (LuckyFor(options.Percent_Time))
                {
                    fragment.Speed = IsOneIn(20)
                        ? RandomDouble(0.1, 0.5)
                        : IsOneIn(2)
                            ? RandomDouble(0.5, 1.0)
                            : RandomDouble(1.0, 2.0);
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
        var fx = false;

        for (var i = 0; i < count; i++)
        {
            var frag = fragments[i];

            var vfx = video && frag.UseOriginalVideo.Janai();
            var afx = audio && frag.UseOriginalAudio.Janai();

            if (vfx) AddVideoFilters(frag, i);
            if (afx) AddAudioFilters(frag, i);

            fx |= afx || vfx;
        }

        if (fx) _args.FilterAppend(";");

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

    private void AddVideoFilters(Fragment frag, int i)
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

    private void AddAudioFilters(Fragment frag, int i)
    {
        _args.Filter($"[{i}:a]");

        if (frag.TimeStretch)
        {
            var change_pitch = IsOneIn(2);
            if (change_pitch)
            {
                var source_rate = probe.GetAudioStream().SampleRate;
                _args.FilterAppend($"asetrate={frag.Speed:F3}*{source_rate}");
            }
            else
            {
                var    speed = frag.Speed;
                while (speed < 0.5)
                {
                    _args.FilterAppend("atempo=0.5");
                    speed *= 2;
                }

                _args.FilterAppend($"atempo={speed:F3}");
            }
        }

        if (frag.SFX)
        {
            var lucky = false;

            // HIGH / LOW PASS
            if (IsOneIn(3))
            {
                var lo = RandomInt(800, 4000);       // upper frequency limit
                var hi = RandomInt(200, lo * 3 / 4); // lower frequency limit
                var lp = IsOneIn(3).Janai();
                var hp = IsOneIn(3).Janai();
                if (lp) _args.FilterAppend( $"lowpass=f={lo}");
                if (hp) _args.FilterAppend($"highpass=f={hi}");

                lucky |= lp || hp;
            }

            if (IsOneIn(3))
            {
                lucky = true;
                _args.FilterAppend($"acompressor=level_in={arr_volume.PickAny()}");
                if (IsOneIn(2)) _args.FilterAppend(":mode=upward");
            }

            // ACRUSHER
            if (IsOneIn(3))
            {
                lucky = true;
                var mode = IsOneIn(2) ? "lin" : "log";
                _args
                    .FilterAppend($"acrusher=bits={RandomInt(2, 8)}")
                    .FilterAppend($":mode={mode}")
                    .FilterAppend($":level_out={arr_volume.PickAny()}")
                    .FilterAppend(  $":samples={arr_sample.PickAny()}");
            }
            // https://ffmpeg.org/ffmpeg-filters.html#acrusher

            if (lucky.Janai()) // add at least something
            {
                _args.FilterAppend($"volume='{RandomInt(2, 15)}*sin(t-{RandomDouble(0, 3.14):F3})*cos(tan(tan(t)))':eval=frame");
            }
        }

        _args.FilterAppend($"[a{i}]");
    }

    private static readonly int[]
        arr_volume = [1, 2, 4, 8, 16, 24, 32, 48, 64],
        arr_sample = [1, 2, 5, 10, 25, 50, 100, 150, 200, 250];
}