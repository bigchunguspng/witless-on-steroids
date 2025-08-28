namespace PF_Tools.FFMpeg;

public class FFProbeResult
{
    public List<Stream> Streams { get; } = [];

    public bool HasVideo() => Streams.Any(x => IsVideo(x));
    public bool HasAudio() => Streams.Any(x => IsAudio(x));

    public Stream? GetPrimaryVideoStream() => Streams.FirstOrDefault(x => IsVideo(x));
    public Stream? GetPrimaryAudioStream() => Streams.FirstOrDefault(x => IsAudio(x));

    private static readonly Predicate<Stream> IsVideo = (x) => x.StreamType is Stream.Type.Video;
    private static readonly Predicate<Stream> IsAudio = (x) => x.StreamType is Stream.Type.Audio;

    public class Stream
    {
        public string  CodecType    { get; set; } = null!;
        public float   Duration     { get; set; } // seconds
        public int     DurationTs   { get; set; } // timebase units
        public float   AvgFramerate { get; set; }
        public float   RawFramerate { get; set; }
        public int?    Width        { get; set; }
        public int?    Height       { get; set; }
        public string? PixFmt       { get; set; }

        public Type StreamType =>
            CodecType is CODEC_audio
                ? Type.Audio
                : CodecType is CODEC_video
                    ? Type.Video
                    : Type.Other;
        /*
        public bool IsLikelyImage =>
            DurationTs <= 1
         || AvgFramerate is 25
         || AvgFramerate is float.NaN && RawFramerate is 90000
         || PixFmt is "yuva420p" or "yuvj420p";
         */

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