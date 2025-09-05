using PF_Tools.Backrooms.Helpers.ProcessRunning;
using PF_Tools.Backrooms.Types;
using PF_Tools.FFMpeg;

namespace PF_Bot.Features.Edit.Shared;

public static class EditingHelpers
{
    public static async
        Task<(string Output, FFProbeResult Probe, FFMpegOutputOptions Options)>
        InitEditing
        (string inputPath, string suffix, string extension) =>
    (
        Output: GetOutputFilePath(inputPath, suffix, extension),
        Probe: await FFProbe.Analyze(inputPath),
        Options: FFMpeg.OutputOptions()
    );

    public static string GetOutputFilePath
        (string inputPath, string suffix, string extension)
    {
        return $"{inputPath.RemoveExtension()}-{suffix}{extension}";
    }
}