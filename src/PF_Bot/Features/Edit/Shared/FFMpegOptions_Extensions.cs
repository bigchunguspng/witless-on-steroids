using PF_Tools.FFMpeg;
using SixLabors.ImageSharp;

namespace PF_Bot.Features.Edit.Shared;

public static class FFMpegOptions_Extensions
{
    public static FFMpegOutputOptions Resize
        (this FFMpegOutputOptions options, Size size)
    {
        return options.Options($"-s {size.Width}x{size.Height}");
    }

    public static FFMpegOutputOptions Crop
        (this FFMpegOutputOptions options, Rectangle rect)
    {
        return options.VF($"crop={rect.Width}:{rect.Height}:{rect.X}:{rect.Y}");
    }

    /// Fixes video playback in Telegram mobile app.
    public static FFMpegOutputOptions FixVideoPlayback
        (this FFMpegOutputOptions options)
    {
        return options.Options(FFMpegOptions.Out_pix_fmt_yuv420p);
    }
}