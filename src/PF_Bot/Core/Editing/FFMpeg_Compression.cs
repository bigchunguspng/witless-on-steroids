using PF_Bot.Features.Edit.Shared;
using PF_Tools.FFMpeg;

namespace PF_Bot.Core.Editing;

public static class FFMpeg_Compression
{
    public static FFMpegOutputOptions ApplyPostNuking
        (this FFMpegOutputOptions options, FFProbeResult probe, Quality quality, bool isVideo = false, bool isSticker = false)
    {
        // todo dedup: RemoveBitrate.CompressVideoAudio
        if (probe.HasVideo && isVideo)
        {
            options
                .FixVideo_Playback()
                .SetCRF(quality.GetCRF());
        }

        if (probe.HasAudio)
        {
            var audio = probe.GetAudioStream();
            var bitrate = quality.GetAudioBitrate_kbps(audio.Bitrate);
            options
                .Options($"-b:a {bitrate}k")
                .FixAudio_InvalidVideo(probe);
            if (probe.HasVideo == false) options.Options("-f mp3");
        }

        var qscale = isSticker
            ? quality.GetQscale_WEBP()
            : quality.GetQscale();

        if (qscale < 13 && !isSticker)
            qscale += RandomInt(0, 10);

        options.Options($"-qscale:v {qscale}");

        return options;
    }
}