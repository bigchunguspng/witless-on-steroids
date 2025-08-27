using System.Diagnostics;
using System.Text;

namespace PF_Tools.Backrooms.Helpers.ProcessRunning;

public static class ProcessStarter
{
    // STARTERS

    public static StartedProcess StartProcess(string file, string arguments)
    {
        return StartProcess_Internal(file, arguments, Output_Save);
    }

    public static StartedProcess StartProcess_WithEcho(string file, string arguments)
    {
        return StartProcess_Internal(file, arguments, Output_SaveAndPrint);
    }

    private static StartedProcess StartProcess_Internal(string file, string arguments, ProcessOutputHandler handler)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = file,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
            },
            SynchronizingObject = null,
        };

        var result = new StartedProcess(process);

        process.OutputDataReceived += (_, e) => handler(e.Data, result.Output);
        process. ErrorDataReceived += (_, e) => handler(e.Data, result.Output);

        process.Start();

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        return result;
    }

    // OUTPUT HANDLERS

    private delegate void ProcessOutputHandler(string? data, StringBuilder output);

    private static void Output_Save
        (string? data, StringBuilder output)
    {
        output.Append(data).Append('\n');
    }

    private static void Output_SaveAndPrint
        (string? data, StringBuilder output)
    {
        output.Append(data).Append('\n');
        Console.WriteLine(data);
    }
}