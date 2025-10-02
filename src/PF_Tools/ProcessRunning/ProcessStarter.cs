using System.Diagnostics;
using System.Text;

namespace PF_Tools.ProcessRunning;

/// Helper class to start processes with output redirection.
public static class ProcessStarter
{
    // STARTERS

    /// Returns a process with redirected stdout/stderr ready to be started.
    public static Process InitProcess
        (string file, string arguments, string directory = "") => new()
    {
        StartInfo = new ProcessStartInfo
        {
            FileName = file, Arguments = arguments,
            WorkingDirectory = directory,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding  = Encoding.UTF8,
        },
    };

    /// Saves all stdout/stderr to result's property <see cref="StartedProcess.Output"/>.
    public static StartedProcess StartProcess
        (string file, string arguments, string directory = "") =>
        StartProcess_WithOutputHandler(file, arguments, directory, Output_Save);

    /// Saves all stdout/stderr to result's property <see cref="StartedProcess.Output"/> and prints them to Console.
    public static StartedProcess StartProcess_WithEcho
        (string file, string arguments, string directory = "") =>
        StartProcess_WithOutputHandler(file, arguments, directory, Output_SaveAndPrint);

    public static StartedProcess StartProcess_WithOutputHandler
        (string file, string arguments, string directory, ProcessOutputHandler handler)
    {
#if DEBUG
        Log($"[RUN] >> {file} {arguments}", LogLevel.Debug, LogColor.Olive);
#endif
        var process = InitProcess(file, arguments, directory);
        var result = new StartedProcess(process);

        process.OutputDataReceived += (_, e) => handler(e.Data, result.Output);
        process. ErrorDataReceived += (_, e) => handler(e.Data, result.Output);

        process.Start();

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        return result;
    }

    // OUTPUT HANDLERS

    public delegate void ProcessOutputHandler(string? data, StringBuilder output);

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