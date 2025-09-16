using PF_Tools.FFMpeg;
using SixLabors.ImageSharp;

namespace PF_Bot.Core.Editing;

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


    // MP4 SPECIFIC

    public static FFMpegOutputOptions SetCRF
        (this FFMpegOutputOptions options, int crf) =>
        options
            .Options(FFMpegOptions.Out_cv_libx264)
            .Options($"-crf {crf}");

    public static FFMpegOutputOptions MP4_EnsureValidSize
        (this FFMpegOutputOptions options, FFProbeResult.Stream video)
        => MP4_EnsureValidSize(options, video, out _);

    public static FFMpegOutputOptions MP4_EnsureValidSize
        (this FFMpegOutputOptions options, FFProbeResult.Stream video, out bool sizeIsValid)
    {
        var size = video.Size;
        var sizeMp4 = size.ValidMp4Size();
        sizeIsValid = size == sizeMp4;
        return sizeIsValid
            ? options
            : options.Resize(sizeMp4);
    }

    public static FFMpegOutputOptions MP4_EnsureSize_Valid_And_Fits
        (this FFMpegOutputOptions options, FFProbeResult.Stream video, int maxSize)
    {
        var size = video.Size;
        var sizeFit  = size.FitSize(maxSize).ValidMp4Size();
        var sizeFits = size == sizeFit;
        return sizeFits
            ? options
            : options.Resize(sizeFit);
    }


    // FIXES

    public static FFMpegOutputOptions Fix_AudioVideo
        (this FFMpegOutputOptions options, FFProbeResult probe)
    {
        if (probe.HasVideo) options.FixVideo_Playback();
        if (probe.HasAudio) options.FixAudio_InvalidVideo(probe);

        return options;
    }

    /// Fixes video playback in Telegram mobile app. Apply only to videos!
    /// Can be used together with -c:v copy.
    public static FFMpegOutputOptions FixVideo_Playback
        (this FFMpegOutputOptions options)
    {
        return options.Options(FFMpegOptions.Out_pix_fmt_yuv420p);
    }

    /// Removes invalid video stream if audio is present.
    /// Almost never happens but still.
    public static FFMpegOutputOptions FixAudio_InvalidVideo
        (this FFMpegOutputOptions options, FFProbeResult probe)
    {
        var video = probe.GetPrimaryVideoStream();
        var audio = probe.GetPrimaryAudioStream();
        if (audio != null && video != null && video.Size == Size.Empty)
        {
            options.Options("-vn");
        }

        return options;
    }
}