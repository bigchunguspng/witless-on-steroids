using PF_Bot.Features_Main.Edit.Commands.Filter;
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

    private record Fragment(TrimCode Trim)
    {
        public bool   SFX   { get; set; }
        public bool   Crop  { get; set; }
        public int    Nuke  { get; set; }
        public double Speed { get; set; } = 1.0;

        public bool TimeStretch => Math.Abs(1.0 - Speed) > 0.005;

        // DON'T FORGET TO UPDATE THESE IF YOU ADD NEW PROP!!!
        public bool UseOriginalVideo => TimeStretch.IsOff() && Crop.IsOff() && Nuke == 0;
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
            fragments.Add(new Fragment(timecode));
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
                    fragments.Add(new Fragment(new TrimCode(start, end)));
                }
            }
        }

        // FX
        foreach (var fragment in fragments)
        {
            if (video)
            {
                fragment.Crop = LuckyFor(options.Percent_Crop);

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

        // FRAG FX
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

        var video = probe.GetVideoStream();

        if (frag.TimeStretch)
        {
            _args.FilterAppend($"setpts={1 / frag.Speed:F3}*PTS");
            _args.FilterAppend($"fps={video.AvgFramerate}");
        }

        if (frag.Crop)
        {
            var og_w = video.Width !.Value;
            var og_h = video.Height!.Value;

            var lucky = false;

            // FLIP
            if (IsOneIn(7))
            {
                lucky = true;
                _args.FilterAppend("hflip");
            }

            // ZOOM IN
            if (IsOneIn(3))
            {
                lucky = true;
                var    zoom_speed = RandomDouble(1, 4);
                var starting_size = RandomDouble(1, 3);
                var formula = LuckyFor(75)
                    ? $"({starting_size:F3}+{zoom_speed:F3}*t*t)" // quadratic
                    : $"({starting_size:F3}+{zoom_speed:F3}*t)";  // linear
                var (off_x, off_y) = IsOneIn(2)
                    ? (0.5, 0.5)                                // zoom to center
                    : (RandomDouble(0, 1), RandomDouble(0, 1)); // zoom to random point
                _args.FilterAppend($"scale=iw*{formula}:ih*{formula}:eval=frame");
                _args.FilterAppend($"crop={og_w}:{og_h}");
                _args.FilterAppend($":({og_w}*{formula}-{og_w})*{off_x:F3}");
                _args.FilterAppend($":({og_h}*{formula}-{og_h})*{off_y:F3}");
            }

            // STRETCH
            if (IsOneIn(3))
            {
                lucky = true;
                var stretch_speed = RandomDouble(2, 4);
                var starting_size = RandomDouble(1, 2);
                var formula = LuckyFor(75)
                    ? $"({starting_size:F3}+{stretch_speed:F3}*t*t)" // quadratic
                    : $"({starting_size:F3}+{stretch_speed:F3}*t)";  // linear
                var widen = IsOneIn(2);
                var scale = widen
                    ? $"scale=iw*{formula}:ih:eval=frame"
                    : $"scale=iw:ih*{formula}:eval=frame";
                _args.FilterAppend(scale);
                _args.FilterAppend($"crop={og_w}:{og_h}");
            }

            // PAN
            if (IsOneIn(3))
            {
                lucky = true;
                var scale = RandomDouble(2, 4);
                var w = (og_w * scale).RoundInt();
                var h = (og_h * scale).RoundInt();
                var w_gap = w - og_w;
                var h_gap = h - og_h;
                var len = frag.Trim.Length;
                var (off_x1, off_y1) = (RandomDouble(0, 1), RandomDouble(0, 1));
                var (off_x2, off_y2) = (RandomDouble(0, 1), RandomDouble(0, 1));
                _args.FilterAppend($"scale={w}:{h}");
                _args.FilterAppend($"crop={og_w}:{og_h}");
                _args.FilterAppend($":(1-t/{len:F3})*{w_gap}*{off_x1:F3}+t/{len:F3}*{w_gap}*{off_x2:F3}");
                _args.FilterAppend($":(1-t/{len:F3})*{h_gap}*{off_y1:F3}+t/{len:F3}*{h_gap}*{off_y2:F3}");
            }

            // SHAKE
            if (IsOneIn(5) || lucky.Janai()) // add at least something
            {
                var size   = RandomDouble(1.05, 1.25);
                var speed  = $"random({RandomInt(0,16)})";
                var offset = $"random({RandomInt(0,16)})";
                _args.FilterAppend($"scale=iw*{size:F3}:ih*{size:F3}");
                _args.FilterAppend($"crop={og_w}:{og_h}");
                _args.FilterAppend($":(iw-out_w)*0.5*(1+sin(t*({speed})-{offset}))");
                _args.FilterAppend($":(ih-out_h)*0.5*(1+sin(t*({speed})+{offset}*2))");
            }

            _args.FilterAppend("setsar=sar=1/1");
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