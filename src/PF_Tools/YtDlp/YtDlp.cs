using PF_Tools.ProcessRunning;

namespace PF_Tools.YtDlp;

public static class YtDlp
{
    public const string DEFAULT_ARGS = "--no-mtime --no-warnings --cookies-from-browser firefox ";

    private const int MIN_HOURS_BEFORE_UPDATES = 8;

    /// If the process fails, updates yt-dlp and tries once more.
    /// <br/> WARNING! Run only in a temporary directory - it can be deleted during retry!
    public static async Task Run(string args, string directory, bool firstTime = true)
    {
        using var memory = new MemoryStream();

        var processResult = await ProcessRunner.Run_WithEcho(YT_DLP, args, directory);
        if (processResult.Failure)
        {
            if (firstTime && TimeToUpdate)
            {
                var updated = await Update();
                if (updated)
                {
                    // todo test if deletion is nesessary
                    Directory.Delete(directory, recursive: true);
                    Directory.CreateDirectory(directory);

                    await Run(args, directory, firstTime: false);

                    return;
                }
            }

            throw new ProcessException(YT_DLP, processResult);
        }
    }

    private static DateTime             LastUpdate;
    private static bool TimeToUpdate => LastUpdate.HappenedWithinLast(TimeSpan.FromHours(MIN_HOURS_BEFORE_UPDATES)) == false;

    private static async Task<bool> Update()
    {
        var processResult = await ProcessRunner.Run_WithEcho(YT_DLP, "--update-to nightly");
        if (processResult.Failure) throw new ProcessException(YT_DLP, processResult);

        LastUpdate = DateTime.Now;

        return processResult.Output.ToString().Contains("Updated yt-dlp");
    }
}