using System.Diagnostics;
using PF_Tools.Backrooms.Helpers.ProcessRunning;

namespace PF_Tools.FFMpeg;

/// Runs FFMpeg processes and manages their priority.
public static class FFMpeg
{
    private const int DEPRIORITIZE_AFTER_SECONDS = 30;
    private const int         IDLE_AFTER_MINUTES =  5;
    private const int         KILL_AFTER_MINUTES = 30;

    /// Creates <see cref="FFMpegArgs"/> instance.
    public static FFMpegArgs Args() => new();

    /// Priority of the process is reduced over time.
    /// After 30 minutes the process is killed.
    public static async Task<ProcessResult> Run(FFMpegArgs args, bool overwrite = true)
    {
        if (overwrite) args.Globals("-y");

        var arguments = args.Build();
        var startedProcess = ProcessStarter.StartProcess(FFMPEG, arguments);
        var process = startedProcess.Process;

        var we = process.WaitForExitAsync();
        var mp = ManagePriority(process);

        await Task.WhenAny(we, mp);

        return new ProcessResult(arguments, startedProcess)
        {
            WasKilled = mp is { IsCompleted: true, Result: true },
        };
    }

    /// True if task was killed.
    private static async Task<bool> ManagePriority(Process process)
    {
        var ts1 = TimeSpan.FromSeconds(DEPRIORITIZE_AFTER_SECONDS);
        var ts2 = TimeSpan.FromMinutes(IDLE_AFTER_MINUTES);
        var ts3 = TimeSpan.FromMinutes(KILL_AFTER_MINUTES);

        await Task.Delay(ts1);
        if (process.HasExited) return false;

        process.PriorityClass = ProcessPriorityClass.BelowNormal;

        await Task.Delay(ts2 - ts1);
        if (process.HasExited) return false;

        process.PriorityClass = ProcessPriorityClass.Idle;

        await Task.Delay(ts3 - ts2);
        if (process.HasExited) return false;

        process.Kill();
        return true;
    }
}