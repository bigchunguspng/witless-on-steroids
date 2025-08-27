using System.Text;
using PF_Bot.Telegram;
using PF_Tools.Backrooms.Helpers;

namespace PF_Bot.Backrooms.Helpers;

public static class YtDlp
{
    public const string DEFAULT_ARGS = "--no-mtime --no-warnings --cookies-from-browser firefox ";

    public static async Task Use(string args, string directory, MessageOrigin origin, bool firstTime = true)
    {
        var exe = "yt-dlp";
        using var memory = new MemoryStream();

        var process = ProcessRunner.StartReadableProcess(exe, args, directory);
        var taskO = ProcessRunner.ReadAndEcho(process.StandardOutput, Console.OpenStandardOutput(), memory);
        var taskE = ProcessRunner.ReadAndEcho(process.StandardError , Console.OpenStandardError() , memory);
        await Task.WhenAll(taskO, taskE);
        await process.WaitForExitAsync();
            
        if (process.ExitCode != 0)
        {
            if (firstTime && !LastUpdate.HappenedWithinLast(TimeSpan.FromHours(8)))
            {
                var updated = await Update();
                if (updated)
                {
                    Directory.GetFiles(directory).ForEach(File.Delete);
                    await Use(args, directory, origin, firstTime: false);

                    return;
                }
            }

            memory.Position = 0;
            using var reader = new StreamReader(memory);
            var output = await reader.ReadToEndAsync();

            var shortMessage = $"{exe} exited with non-zero exit-code: {process.ExitCode}";
            var sb = new StringBuilder(shortMessage);
            sb.Append($"\n\nВЫВОД:\n{output}");
            var files = new DirectoryInfo(directory).GetFiles();
            if (files.Length > 0)
            {
                sb.Append("\n\nСКАЧАННЫЕ ФАЙЛЫ (Можно достать командой /peg):\n");
                foreach (var file in files)
                {
                    sb.Append($"\n{file.Length.ReadableFileSize(),12}   {file.FullName}");
                }
            }

            var message = sb.ToString();
            Bot.Instance.SendErrorDetails(origin, $"{exe} {args}", message);
            throw new Exception(shortMessage);
        }
    }

    private static DateTime LastUpdate;

    private static async Task<bool> Update()
    {
        using var memory = new MemoryStream();
        var process = ProcessRunner.StartReadableProcess("yt-dlp", "--update-to nightly");
        var taskO = ProcessRunner.ReadAndEcho(process.StandardOutput, Console.OpenStandardOutput(), memory);
        var taskE = ProcessRunner.ReadAndEcho(process.StandardError , Console.OpenStandardError() , memory);
        await Task.WhenAll(taskO, taskE);
        await process.WaitForExitAsync();

        LastUpdate = DateTime.Now;

        memory.Position = 0;
        using var reader = new StreamReader(memory);
        var output = await reader.ReadToEndAsync();
        return output.Contains("Updated yt-dlp");
    }
}