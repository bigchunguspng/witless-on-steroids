using PF_Tools.Backrooms.Helpers.ProcessRunning;
using PF_Tools.Backrooms.Types;
using PF_Tools.FFMpeg;

namespace PF_Bot.Features.Edit.Shared;

public static class EditingHelpers
{
    // SIMPLE STUFF

    public static string GetOutputFilePath
        (string inputPath, string suffix, string extension)
    {
        return $"{inputPath.RemoveExtension()}-{suffix}{extension}";
    }


    // FFPROBE

    public static async Task<FFProbeResult.Stream> 
        GetVideoStream
        (string filePath)
    {
        var probe = await FFProbe.Analyze(filePath);
        return probe.GetVideoStream();
    }

    public static FFProbeResult.Stream 
        GetVideoStream
        (this FFProbeResult probe) =>
        probe.GetPrimaryVideoStream() ?? throw new UnexpectedException("FILE HAS NO VIDEO STREAM");

    // FFMPEG

    /// Use this to automaticaly throw <see cref="ProcessException"/> on failure.
    public static async Task
        FFMpeg_Run(FFMpegArgs args)
    {
        var result = await FFMpeg.Run(args);
        if (result.Failure) throw new ProcessException(FFMPEG, result);
    }
}