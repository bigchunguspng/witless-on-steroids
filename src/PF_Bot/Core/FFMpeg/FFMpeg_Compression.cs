using PF_Bot.Features.Edit.Shared;
using PF_Tools.FFMpeg;

namespace PF_Bot.Core.FFMpeg;

public static class FFMpeg_Compression
{
    public static FFMpegOutputOptions ApplyPostNuking
        (this FFMpegOutputOptions options, FFProbeResult probe, int compression = 0, bool isVideo = false)
    {
        var factor = isVideo
            ? compression
            : compression > 26
                ? compression
                : Math.Min(31, compression + RandomInt(0, 10));

        // todo dedup: RemoveBitrate.CompressVideoAudio
        if (probe.HasVideo)
        {
            options
                .Options(FFMpegOptions.Out_cv_libx264)
                .Options($"-crf {factor}");
        }

        if (probe.HasAudio)
        {
            var audio = probe.GetAudioStream();
            var bitrate = GetAudioBitrate(audio.Bitrate, factor);
            options.Options($"-b:a {bitrate}");
            if (probe.HasVideo == false) options.Options("-f mp3");
        }

        options.Fix_AudioVideo(probe);
        options.Options($"-qscale:v {factor}");

        return options;
    }

    private static int GetAudioBitrate(int bitrate, int factor)
    {
        if (bitrate <= 0) return 154 - 3 * factor;

        var quality = (21 - factor) / 21F;
        return Math.Max((int)(bitrate * quality), 91);
    }
}