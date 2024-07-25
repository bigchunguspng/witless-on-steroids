using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Witlesss.MediaTools;

namespace Witlesss.Commands.Meme;

public class MemeFileRequest(string path, string oututEnding, int quality)
{
    public string SourcePath { get; set; } = path;
    public string TargetPath { get; } = path.ReplaceExtension(oututEnding);

    public int Quality { get; set; } = quality; // 0 - 100

    public MemeSourceType  Type { get; init; }
    public bool ExportAsSticker { get; init; }
    public bool  ConvertSticker { get; init; }

    public bool IsSticker => Type == MemeSourceType.Sticker;

    /// <summary>
    /// Constant Rate Factor (for MP4 compresion).<br/>
    /// 0 - lossless, 23 - default, 51 - worst possible.
    /// </summary>
    public int GetCRF()
    {
        return Quality > 80
            ? 0
            : 51 - (int)(0.42 * Quality); // 17 - 51
    }

    /// <summary>
    /// Quality of JPEG image or MP3 audio.<br/>
    /// 1 - highest, 2-3 - default (JPEG), 31 - lowest.
    /// </summary>
    public int GetQscale()
    {
        return 31 - (int)(0.29 * Quality); // 2 - 31
    }

    public Image<Rgba32> GetVideoSnapshot()
    {
        return Image.Load<Rgba32>(FFMpegXD.Snapshot(SourcePath));
    }
}

public enum MemeSourceType
{
    Image, Sticker, Video
}
