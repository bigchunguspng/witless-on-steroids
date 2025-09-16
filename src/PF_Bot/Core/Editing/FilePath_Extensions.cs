using PF_Tools.FFMpeg;
using FileEditingKit =
(
    PF_Tools.Backrooms.Types.FilePath Output,
    PF_Tools.FFMpeg.FFProbeResult Probe,
    PF_Tools.FFMpeg.FFMpegOutputOptions Options
);

namespace PF_Bot.Core.Editing;

public static class FilePath_Extensions
{
    public static async Task<FileEditingKit> InitEditing
        (this FilePath inputPath, string suffix, string extension) =>
    (
        Output: GetOutputFilePath(inputPath, suffix, extension),
        Probe: await FFProbe.Analyze(inputPath),
        Options: FFMpeg.OutputOptions()
    );

    public static FilePath GetOutputFilePath
        (this FilePath inputPath, string suffix, string extension)
    {
        return inputPath.Suffix(suffix, Desert.GetSand(), extension).MakeUnique();
    }
}