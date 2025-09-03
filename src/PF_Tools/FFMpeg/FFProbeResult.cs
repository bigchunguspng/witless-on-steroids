using SixLabors.ImageSharp;

namespace PF_Tools.FFMpeg;

public class FFProbeResult(List<FFProbeResult.Stream> streams)
{
    public IReadOnlyCollection<Stream> Streams { get; } = streams;

    public bool HasVideo = streams.Any(x => IsVideo(x));
    public bool HasAudio = streams.Any(x => IsAudio(x));

    public Stream? GetPrimaryVideoStream() => Streams.FirstOrDefault(x => IsVideo(x));
    public Stream? GetPrimaryAudioStream() => Streams.FirstOrDefault(x => IsAudio(x));

    private static readonly Predicate<Stream> IsVideo = (x) => x.StreamType is Stream.Type.Video;
    private static readonly Predicate<Stream> IsAudio = (x) => x.StreamType is Stream.Type.Audio;

    public TimeSpan Duration => TimeSpan.FromSeconds(Streams.Max(x => x.Duration));

    public class Stream
    {
        public string  CodecType    { get; set; } = null!;
        public float   Duration     { get; set; } // seconds
        public int     DurationTs   { get; set; } // timebase units
        public int     Bitrate      { get; set; }
        public float   AvgFramerate { get; set; }
        public float   RawFramerate { get; set; }
        public int?    Width        { get; set; }
        public int?    Height       { get; set; }
        public string? PixFmt       { get; set; }

        public Size Size => new(Width ?? 0, Height ?? 0);

        public Type StreamType =>
            CodecType is CODEC_audio
                ? Type.Audio
                : CodecType is CODEC_video
                    ? Type.Video
                    : Type.Other;

        public bool IsLikelyImage => Bitrate <= 0;

            /*DurationTs <= 1
         || AvgFramerate is 25
         || AvgFramerate is float.NaN && RawFramerate is 90000
         || PixFmt is "yuva420p" or "yuvj420p";*/

        public enum Type
        {
            Audio,
            Video,
            Other,
        }

        private const string CODEC_video = "video";
        private const string CODEC_audio = "audio";
    }
}