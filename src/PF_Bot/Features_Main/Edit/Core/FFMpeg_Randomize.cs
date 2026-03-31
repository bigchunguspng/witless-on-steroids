using PF_Tools.FFMpeg;

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
        (RandomizeOptions options, TimeSelection selection = default)
    {
        var soundOnly = SoundOnly();
        var timecodes = GetTrimCodes(0.1, 0.25, soundOnly, selection, go_back_chance_mul: 200); // todo random-specific slicing code
        var fragments = GenerateSchematic(timecodes, options, soundOnly);

        // todo
        // repeats - not equal in length, may be offset
        // slice jump back and forth with higher chance

        AddInputs(fragments);
        AddFilter(fragments, soundOnly);

        return _args;
    }

    private record Fragment(TrimCode Trim, int Copy) // repeat number, 0 - first occurence
    {
        public int Nuke { get; set; }

        public bool UseOriginalVideo => Nuke == 0;
        public bool UseOriginalAudio => true;
    }

    private List<Fragment> GenerateSchematic
        (List<TrimCode> timecodes, RandomizeOptions options, bool soundOnly)
    {
        var fragments = new List<Fragment>(timecodes.Count);

        var video = probe.HasVideo && soundOnly.Janai();
        var audio = probe.HasAudio;

        // MAP + REPEATS
        foreach (var timecode in timecodes)
        {
            fragments.Add(new Fragment(timecode, 0));
            var repeat = Fortune.LuckyFor(options.Percent_Repeats);
            if (repeat)
            {
                var (from, to) = options.Range_Repeats;
                var number = Fortune.RandomInt(from, to);
                for (var i = 0; i < number; i++)
                {
                    fragments.Add(new Fragment(timecode, i + 1));
                }
            }
        }

        // FX
        foreach (var fragment in fragments)
        {
            if (video)
            {
                var nuke = Fortune.LuckyFor(options.Percent_Nuke); // todo only if video
                if (nuke)
                {
                    var (from, to) = options.Range_Nuke;
                    fragment.Nuke = Fortune.RandomInt(from, to);
                }
            }

            if (audio)
            {

            }
        }

        return fragments;
    }

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
                // ...
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