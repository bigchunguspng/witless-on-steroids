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
        var startInfo = new ProcessStartInfo
        {
            FileName = exe, Arguments = args,
            WorkingDirectory = directory,
            RedirectStandardOutput = redirect,
            RedirectStandardError  = redirect,
        };
        var process = new Process { StartInfo = startInfo };

#if DEBUG
        Log($"[{exe.ToUpper()}] >> {args}", ConsoleColor.DarkYellow);
#endif

        process.Start();
        return process;
    }
}

public static class YtDlp
{
    public const string DEFAULT_ARGS = "--no-mtime --no-warnings --cookies-from-browser firefox ";

    public static Task Use(string args, string directory)
    {
        return SystemHelpers.StartProcess("yt-dlp", args, directory, redirect: false).WaitForExitAsync();
    }
}