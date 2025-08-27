using System.Diagnostics;
using PF_Tools.FFMpeg;

namespace PF_Bot.Backrooms.Helpers;

/// Runs FFMpeg processes and manages their priority.
public static class FFMpegRunner
{
    private const int DEPRIORITIZE_AFTER_SECONDS = 30;
    private const int         IDLE_AFTER_MINUTES =  5;
    private const int         KILL_AFTER_MINUTES = 30;

    /// Priority of the process is reduced over time.
    /// After 30 minutes the process is killed.
    public static async Task<FFMpegResult> Run(FFMpegArgs args, bool overwrite = true)
    {
        if (overwrite) args.Globals("-y");

        var (process, result) = FFMpeg.StartProcess(args);

        var mp = ManagePriority(process);
        var we = process.WaitForExitAsync();

        await Task.WhenAny(mp, we);

        result.ExitCode = process.ExitCode;
        result.WasKilled = mp is { IsCompleted: true, Result: true };

        return result;
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