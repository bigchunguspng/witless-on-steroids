using PF_Tools.FFMpeg;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace PF_Bot.Features_Main.Memes.Core.Shared;

public class MemeRequest(MemeSourceType type, FilePath source, FilePath target, byte quality, float press)
{
    public MemeSourceType  Type { get; } = type;
    public FilePath  SourcePath { get; } = source;
    public FilePath  TargetPath { get; } = target;
    public Quality      Quality { get; } = quality;
    public float          Press { get; } = Math.Clamp(press, 0, 1);

    public bool ExportAsSticker { get; set; }
    public bool     JpegSticker { get; set; }

    public bool IsSticker => Type == MemeSourceType.Sticker;
    public bool IsVideo   => Type == MemeSourceType.Video;

    public async Task<FFProbeResult> ProbeSource
        () => await FFProbe.Analyze(SourcePath);

    public async Task<Image<Rgba32>> GetVideoSnapshot()
    {
        var temp = GetTempFileName("png");
        await FFMpeg.Command(SourcePath, temp, o => o.Options("-frames:v 1")).FFMpeg_Run();
        return Image.Load<Rgba32>(temp);
    }
}

public enum MemeSourceType
{
    Image, Sticker, Video,
}
