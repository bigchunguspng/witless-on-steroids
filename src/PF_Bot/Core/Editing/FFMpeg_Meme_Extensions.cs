using PF_Tools.FFMpeg;

namespace PF_Bot.Core.Editing;

public static class FFMpeg_Meme_Extensions
{
    /// Image/video: squize and stretch back (complex filter).
    public static void Meme_HydraulicPress
        (this FFMpegArgs args, float press)
    {
        if (press == 0) return;

        args.FilterAppend($"scale=ceil((iw*{press})/2)*2:ceil((ih*{press})/2)*2");
        args.FilterAppend($"scale=ceil((iw/{press})/2)*2:ceil((ih/{press})/2)*2");
    }

    /// Compress video and audio quality.
    public static void Meme_Compression
        (this FFMpegOutputOptions options, Quality quality, FFProbeResult probe)
    {
        options
            .FixVideo_Playback()
            .SetCRF(quality.GetCRF());

        if (probe.HasAudio)
        {
            var audio = probe.GetAudioStream();
            var bitrate = quality.GetAudioBitrate_kbps(audio.Bitrate);
            options.Options($"-b:a {bitrate}k");
        }
    }
}