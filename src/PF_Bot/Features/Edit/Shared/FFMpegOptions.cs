using SixLabors.ImageSharp;

namespace PF_Bot.Features.Edit.Shared;

public static class FFMpegOptions
{
    public const string Out_cv_copy    = "-c:v copy";
    public const string Out_cv_libx264 = "-c:v libx264";
    public const string Out_crf_30     = "-crf 30";
    public const string Out_pix_fmt_yuv420p = "-pix_fmt yuv420p";

    public static readonly Size      VIDEONOTE_SIZE = new        (384, 384);
    public static readonly Rectangle VIDEONOTE_CROP = new(56, 56, 272, 272);
}