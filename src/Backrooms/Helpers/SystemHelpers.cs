using System.Diagnostics;
using System.Text;

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
            StandardOutputEncoding = redirect ? Encoding.UTF8 : null,
            StandardErrorEncoding  = redirect ? Encoding.UTF8 : null
        };
        var process = new Process { StartInfo = startInfo };

#if DEBUG
        Log($"[{exe.ToUpper()}] >> {args}", LogLevel.Debug, 3);
#endif

        process.Start();
        return process;
    }

    public static async Task ReadAndEcho(StreamReader input, Stream output1, Stream output2)
    {
        var buffer = new byte[4096];
        int bytesRead;

        while ((bytesRead = await input.BaseStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            await output1.WriteAsync(buffer, 0, bytesRead);
            await output1.FlushAsync();
            await output2.WriteAsync(buffer, 0, bytesRead);
            await output2.FlushAsync();
        }
    }
}

public static class YtDlp
{
    public const string DEFAULT_ARGS = "--no-mtime --no-warnings --cookies-from-browser firefox ";

    public static async Task Use(string args, string directory, MessageOrigin origin, bool firstTime = true)
    {
        var exe = "yt-dlp";
        using var memory = new MemoryStream();

        var process = SystemHelpers.StartProcess(exe, args, directory, redirect: true);
        var taskO = SystemHelpers.ReadAndEcho(process.StandardOutput, Console.OpenStandardOutput(), memory);
        var taskE = SystemHelpers.ReadAndEcho(process.StandardError , Console.OpenStandardError() , memory);
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
        var process = SystemHelpers.StartProcess("yt-dlp", "-U", redirect: true);
        var taskO = SystemHelpers.ReadAndEcho(process.StandardOutput, Console.OpenStandardOutput(), memory);
        var taskE = SystemHelpers.ReadAndEcho(process.StandardError , Console.OpenStandardError() , memory);
        await Task.WhenAll(taskO, taskE);
        await process.WaitForExitAsync();

        LastUpdate = DateTime.Now;

        memory.Position = 0;
        using var reader = new StreamReader(memory);
        var output = await reader.ReadToEndAsync();
        return output.Contains("Updated yt-dlp");
    }
}