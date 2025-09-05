using PF_Bot.Tools_Legacy.FFMpeg;
using PF_Tools.FFMpeg;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PF_Bot.Features.Generate.Memes.Core;

public class MemeFileRequest(MessageOrigin origin, MemeSourceType type, FilePath path, string outputEnding, int quality, float press)
{
    public MessageOrigin Origin { get; }      = origin;
    public MemeSourceType  Type { get; }      = type;
    public FilePath  SourcePath { get; set; } = path;
    public FilePath  TargetPath { get; }      = path.ChangeEnding(outputEnding);
    public int          Quality { get; }      = quality.Clamp(0, 100);
    public float          Press { get; }      = Math.Clamp(press, 0, 1);

    public bool ExportAsSticker { get; set; }
    public bool     JpegSticker { get; set; }

    public bool IsSticker => Type == MemeSourceType.Sticker;
    public bool IsVideo   => Type == MemeSourceType.Video;

    /// <summary>
    /// Constant Rate Factor (for MP4 compression).<br/>
    /// 0 - lossless, 23 - default, 51 - worst possible.
    /// </summary>
    public int GetCRF()
    {
        return Math.Clamp(51 - (int)(0.35 * Quality), 23, 51);
    }

    /// <summary>
    /// Quality of JPEG image or MP3 audio.<br/>
    /// 1 - highest, 2-3 - default (JPEG), 31 - lowest.
    /// </summary>
    public int GetQscale()
    {
        return 31 - (int)(0.29 * Quality); // 2 - 31
    }

    public async Task<FFProbeResult> ProbeSource
        () => await FFProbe.Analyze(SourcePath);

    public F_Process UseFFMpeg() => new(SourcePath, Origin);

    public Image<Rgba32> GetVideoSnapshot()
    {
        return Image.Load<Rgba32>(FFMpegXD.Snapshot(SourcePath));
    }
}

public enum MemeSourceType
{
    Image, Sticker, Video
}
