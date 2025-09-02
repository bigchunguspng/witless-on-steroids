using PF_Tools.Backrooms.Helpers.ProcessRunning;
using PF_Tools.Backrooms.Types;
using PF_Tools.FFMpeg;

namespace PF_Bot.Features.Edit.Shared;

public static class EditingHelpers
{
    public static string GetOutputFilePath
        (string inputPath, string suffix, string extension)
    {
        return $"{inputPath.RemoveExtension()}-{suffix}{extension}";
    }
}