using System.Diagnostics;
using System.Text;

namespace PF_Tools.Backrooms.Helpers;

public static class ProcessRunner
{
    public static Process StartReadableProcess(string cmd, string args, string directory = "")
    {
        return StartProcess(cmd, args, directory, redirect: true);
    }

    public static Process StartProcess(string cmd, string args, string directory = "", bool redirect = false)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = cmd, Arguments = args,
            WorkingDirectory = directory,
            UseShellExecute = false,
            RedirectStandardOutput = redirect,
            RedirectStandardError  = redirect,
            StandardOutputEncoding = redirect ? Encoding.UTF8 : null,
            StandardErrorEncoding  = redirect ? Encoding.UTF8 : null,
        };
        var process = new Process { StartInfo = startInfo };

#if DEBUG
        Log($"[{exe.ToUpper()}] >> {args}", LogLevel.Debug, LogColor.Olive);
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