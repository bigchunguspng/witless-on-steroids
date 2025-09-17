using PF_Bot.Core.Memes.Shared;
using PF_Tools.FFMpeg;
using static PF_Tools.Backrooms.Helpers.Fortune;

namespace PF_Bot.Core.Editing;

public class FFMpeg_Nuke(FFProbeResult probe, MemeRequest request)
{
    private readonly FFMpegArgs             _args = new();
    private readonly FFMpegOutputOptions _options = new();

    public FFMpegArgs Nuke(int depth = 1)
    {
        _args.Input(request.SourcePath);
        _args.Out  (request.TargetPath, _options);
        _args.Filter("[v:0]");

        if (request.IsVideo)
        {
            var size = probe.GetVideoStream().Size;
            var sizeMp4 =  size.ValidMp4Size();
            if (sizeMp4 != size)
            {
                _args.FilterAppend($"scale={sizeMp4.Width}:{sizeMp4.Height}");
            }
        }

        _args.Meme_HydraulicPress(request.Press);

        for (var i = 0; i < depth; i++)
        {
            DropNuke(request.IsVideo);
        }

        SetQuality();

        return _args;
    }

    private void DropNuke(bool isVideo = false)
    {
        // VIGNETTE
        if (isVideo && IsOneIn(4))
        {
            _args.FilterAppend($"vignette={RandomDouble(0.1, 0.5)}");
        }
        // https://ffmpeg.org/ffmpeg-filters.html#vignette-1

        // PIXELIZE
        if (IsOneIn(isVideo ? 4 : 8))
        {
            var size = probe.GetVideoStream().Size;
            var p = Math.Max(2, Math.Min(size.Width, size.Height) / RandomInt(60, 120));
            _args.FilterAppend($"pixelize={p}:{p}:p=3");
        }
        // https://ffmpeg.org/ffmpeg-filters.html#pixelize

        // AMPLIFY
        if (isVideo && IsOneIn(4))
        {
            var radius = RandomInt(1, 5);
            var factor = RandomInt(1, 5);
            var zeroes = RandomInt(0, 3);

            _args.FilterAppend($"amplify={radius}:factor={factor}");
            _args.FilterAppend($":threshold=1{new string('0', zeroes)}"); // 1-10-100-1000

            if (zeroes == 3) // if threshold = 1000
            {
                var values = new[] { 1, 10, 25, 50 };
                _args.FilterAppend($":tolerance={values.PickAny()}");
            }
        }
        // https://ffmpeg.org/ffmpeg-filters.html#amplify

        // HUE SATURATION
        var hue        = RandomInt(-25,  25); // [-180 - 180]
        var strength   = RandomInt(  1, 100);
        var saturation = RandomDouble(-1, 1); // was 0, 0.5
        var intensity  = RandomDouble(-1, 1); // 1 - 100 // was 1, 14
        _args.FilterAppend($"huesaturation={hue}");
        _args.FilterAppend($":saturation={saturation}");
        _args.FilterAppend($":intensity={intensity}");
        _args.FilterAppend($":strength={strength}");

        if (IsOneIn(4))
        {
            var colors = new[] { 'r', 'g', 'b', 'c', 'm', 'y' };
            var selectedColors = colors.Where(_ => IsOneIn(3)).ToArray();
            if (selectedColors.Length > 0)
            {
                _args.FilterAppend($":colors={string.Join('+', selectedColors)}");
            }
        }

        if (IsOneIn(4))
        {
            var colors = new[] { "rw", "gw", "bw" };
            var selectedColors = colors.Where(_ => IsOneIn(RandomInt(2, 3))).ToArray();
            foreach (var color in selectedColors)
            {
                _args.FilterAppend($":{color}={RandomDouble(0, 1)}");
            }
        }
        // https://ffmpeg.org/ffmpeg-filters.html#huesaturation

        // UNSHARP
        var lumaMatrixSize = RandomInt(1, 11) * 2 + 1; // [3 - 23], odd only, lx + ly <= 26
        var lumaAmount = RandomDouble(-1.5, 1.5); // [-1.5 - 1.5]

        var b = IsOneIn(2);
        var s = Math.Min(lumaMatrixSize, 26 - lumaMatrixSize);
        _args.FilterAppend("unsharp");
        _args.FilterAppend($"=lx={(b ? s : lumaMatrixSize)}");
        _args.FilterAppend($":ly={(b ? lumaMatrixSize : s)}");
        _args.FilterAppend($":la={lumaAmount}");
        // https://ffmpeg.org/ffmpeg-filters.html#unsharp-1

        // NOISE
        var n_min = isVideo ? 10 : 25;
        var n_max = isVideo ? 45 : 100;

        _args           .FilterAppend($"noise=c0s={RandomInt(n_min, n_max)}"); // [0 - 100]
        if (IsOneIn(4)) _args.FilterAppend($":c1s={RandomInt(n_min, n_max)}"); // yellow-blue
        if (IsOneIn(4)) _args.FilterAppend($":c2s={RandomInt(n_min, n_max)}"); // red-green

        _args.FilterAppend(":allf=t");
        var flags = new[] { 'u', 'p', 'a' };
        var selectedFlags = flags.Where(_ => IsOneIn(2)).ToArray();
        foreach (var flag in selectedFlags)
        {
            _args.FilterAppend($"+{flag}");
        }
        // https://ffmpeg.org/ffmpeg-filters.html#noise
    }

    private void SetQuality()
    {
        var quality = request.Quality;

        if (request.IsVideo)
            _options.Meme_Compression(quality, probe);

        var sticker = request is { IsSticker: true, ExportAsSticker: true };
        if (sticker)
        {
            var qscale = quality.GetQscale_WEBP();
            _options.Options($"-qscale:v {qscale}");
        }
    }
}