using System.Diagnostics;

namespace Witlesss.Backrooms.Helpers;

public static class SystemHelpers
{
    public static Process StartReadableProcess(string exe, string args, string directory = "")
    {
        return StartProcess(exe, args, directory, redirect: true);
    }

    public static Process StartProcess(string exe, string args, string directory = "", bool redirect = false)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = exe,
                Arguments = args,
                WorkingDirectory = directory,
                RedirectStandardOutput = redirect,
            }
        };
        process.Start();
        return process;
    }
}