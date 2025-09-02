using PF_Tools.Backrooms.Helpers.ProcessRunning;
using PF_Tools.Backrooms.Types;

namespace PF_Tools.FFMpeg;

public static class FFProbe
{
    private const string
        CODEC_TYPE  = "codec_type",
        DURATION    = "duration",
        DURATION_TS = "duration_ts",
        RAW_RATE    =   "r_frame_rate",
        AVG_RATE    = "avg_frame_rate",
        W = "width",
        H = "height",
        PIXFMT = "pix_fmt";

    private const string ENTRIES  = $"{CODEC_TYPE},{DURATION},{DURATION_TS},{RAW_RATE},{AVG_RATE},{W},{H},{PIXFMT}";
    private const string ARGS     = $"-v error -show_entries stream={ENTRIES}";


    // WRAPPERS

    /// Returns primary stream or throws <see cref="UnexpectedException"/>.
    public static async Task<FFProbeResult.Stream> 
        GetVideoStream
        (string filePath)
    {
        var probe = await Analyze(filePath);
        return probe.GetVideoStream();
    }

    /// Returns primary stream or throws <see cref="UnexpectedException"/>.
    public static FFProbeResult.Stream
        GetVideoStream
        (this FFProbeResult probe) =>
        probe.GetPrimaryVideoStream() ?? throw new UnexpectedException("FILE HAS NO VIDEO STREAM");


    // LOGIC

    public static async Task<FFProbeResult> Analyze(string filePath)
    {
        var arguments = $@"{ARGS} ""{filePath}""";
        var (stdout, _) = await ProcessRunner.Run_GetOutput(FFPROBE, arguments);
        return await ParseOutput(stdout);
    }

    private static async Task<FFProbeResult> ParseOutput(string stdout)
    {
        var result = new FFProbeResult();

        using var reader = new StringReader(stdout);
        while (await reader.ReadLineAsync() is { } line)
        {
            ParseLine(line, result.Streams);
        }

        return result;
    }

    private static void ParseLine(string line, List<FFProbeResult.Stream> streams)
    {
        if (line == "[STREAM]")
        {
            streams.Add(new FFProbeResult.Stream());
        }
        else if (streams.Count > 0 && line.Contains('='))
        {
            var parts = line.Split('=', 2);
            var key   = parts[0];
            var value = parts[1];

            switch (key)
            {
                case CODEC_TYPE:  streams[^1].CodecType    = value;                break; // string
                case PIXFMT:      streams[^1].PixFmt       = value;                break; // string
                case DURATION:    streams[^1].Duration     = ParseFloat_NA(value); break; // float      | N/A -> NaN
                case DURATION_TS: streams[^1].DurationTs   = ParseInt___NA(value); break; // int        | N/A -> -1
                case W:           streams[^1].Width        = ParseInt___NA(value); break; // int        | -
                case H:           streams[^1].Height       = ParseInt___NA(value); break; // int        | -
                case RAW_RATE:    streams[^1].RawFramerate = ParseFps___NA(value); break; // 24000/1001 | N/A -> NaN
                case AVG_RATE:    streams[^1].AvgFramerate = ParseFps___NA(value); break; // 24000/1001 | N/A -> NaN
            }
        }
    }

    private static int
        ParseInt___NA(string raw) =>   int.TryParse(raw, out var value) ? value : -1;

    private static float
        ParseFloat_NA(string raw) => float.TryParse(raw, out var value) ? value : float.NaN;

    private static float 
        ParseFps___NA(string fps)
    {
        if (fps.Contains('/'))
        {
            var parts = fps.Split('/');
            return float.Parse(parts[0])
                 / float.Parse(parts[1]); // 0/0 -> NaN
        }

        return float.TryParse(fps, out var value)
            ? value
            : float.NaN;
    }
}