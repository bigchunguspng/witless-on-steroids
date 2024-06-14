using System.Diagnostics;

namespace Witlesss.Backrooms;

public static class SystemHelpers
{
    public static Process StartedReadableProcess(string exe, string args)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = exe,
                Arguments = args,
                RedirectStandardOutput = true
            }
        };
        process.Start();
        return process;
    }
}