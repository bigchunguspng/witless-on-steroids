using System.Drawing;
using System.Text;
using FFMpegCore;
using Witlesss.Backrooms;

namespace Witlesss.MediaTools;

// todo: randomize effects order, introduce depth, add options support?
public partial class F_Process
{
    public F_Process Nuke     (int qscale)         => ApplyEffects(o => NukeArgs(o, qscale));

    public F_Process NukeVideo(Size size, int crf) => ApplyEffects(o => NukeArgs(o.Resize(size), crf, isVideo: true));

    private void NukeArgs(FFMpegArgumentOptions o, int compression = 0, bool isVideo = false)
    {
        var sb = new StringBuilder("-filter_complex \"[v:0]");

        // VIGNETTE
        if (isVideo && IsOneIn(4))
        {
            sb.Append("vignette=").Append(RandomDouble(0.1, 0.5)).Append(',');
        }
        // https://ffmpeg.org/ffmpeg-filters.html#vignette-1

        // PIXELIZE
        if (IsOneIn(isVideo ? 4 : 8))
        {
            var i = GetMediaInfo();
            var p = Math.Max(2, Math.Min(i.Video.Width, i.Video.Height) / RandomInt(60, 120));
            sb.Append("pixelize=").Append(p).Append(':').Append(p).Append(":p=3,");
        }
        // https://ffmpeg.org/ffmpeg-filters.html#pixelize

        // AMPLIFY
        if (isVideo && IsOneIn(4))
        {
            sb.Append("amplify=").Append(RandomInt(1, 5)); // radius
            sb.Append(":factor=").Append(RandomInt(1, 5));

            var zeroes = RandomInt(0, 3);
            sb.Append(":threshold=1").Append(new string('0', zeroes)); // 1-10-100-1000
            if (zeroes == 3) // if threshold = 1000
            {
                var values = new[] { 1, 10, 25, 50 };
                sb.Append(":tolerance=").Append(values.PickAny());
            }

            sb.Append(',');
        }
        // https://ffmpeg.org/ffmpeg-filters.html#amplify

        // HUE SATURATION
        sb.Append("huesaturation=").Append(RandomInt(-25, 25)); // [-180 - 180]
        sb.Append(":saturation=").Append(RandomDouble(-1, 1));
        sb.Append(":intensity=").Append(RandomDouble(-1, 1)); // was 0, 0.5
        sb.Append(":strength=").Append(RandomInt(1, 100)); // 1 - 100 // was 1, 14
        if (IsOneIn(4))
        {
            var colors = new[] { 'r', 'g', 'b', 'c', 'm', 'y' };
            var selectedColors = colors.Where(_ => IsOneIn(3)).ToArray();
            if (selectedColors.Length > 0)
            {
                sb.Append(":colors=").Append(string.Join('+', selectedColors));
            }
        }

        if (IsOneIn(4))
        {
            var colors = new[] { "rw", "gw", "bw" };
            var selectedColors = colors.Where(_ => IsOneIn(RandomInt(2, 3))).ToArray();
            foreach (var color in selectedColors)
            {
                sb.Append(":").Append(color).Append("=").Append(RandomDouble(0, 1));
            }
        }

        sb.Append(",");
        // https://ffmpeg.org/ffmpeg-filters.html#huesaturation

        // UNSHARP
        var lumaMatrixSize = RandomInt(1, 11) * 2 + 1; // [3 - 23], odd only, lx + ly <= 26
        var lumaAmount = RandomDouble(-1.5, 1.5); // [-1.5 - 1.5]

        sb.Append("unsharp");
        var b = IsOneIn(2);
        var s = Math.Min(lumaMatrixSize, 26 - lumaMatrixSize);
        sb.Append("=lx=").Append(b ? s : lumaMatrixSize);
        sb.Append(":ly=").Append(b ? lumaMatrixSize : s);
        sb.Append(":la=").Append(lumaAmount).Append(",");
        // https://ffmpeg.org/ffmpeg-filters.html#unsharp-1

        // NOISE
        var n_min = isVideo ? 10 : 25;
        var n_max = isVideo ? 45 : 100;

        sb.Append("noise").Append("=c0s=").Append(RandomInt(n_min, n_max)); // [0 - 100]
        if (IsOneIn(4)) sb.Append(":c1s=").Append(RandomInt(n_min, n_max)); // yellow-blue
        if (IsOneIn(4)) sb.Append(":c2s=").Append(RandomInt(n_min, n_max)); // red-green

        sb.Append(":allf=t");
        var flags = new[] { 'u', 'p', 'a' };
        var selectedFlags = flags.Where(_ => IsOneIn(2)).ToArray();
        foreach (var flag in selectedFlags)
        {
            sb.Append('+').Append(flag);
        }

        sb.Append("\"");
        // https://ffmpeg.org/ffmpeg-filters.html#noise


        var factor = isVideo
            ? compression
            : compression > 26
                ? compression
                : Math.Min(31, compression + RandomInt(0, 10));

        if (isVideo) AddCompression(o, factor);
        o.WithQscale(factor).WithCustomArgument(sb.ToString());


        string RandomDouble(double min, double max) => Extensions.RandomDouble(min, max).Format();
    }
}