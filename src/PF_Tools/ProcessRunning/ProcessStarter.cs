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
        StartProcess_WithOutputHandler(file, arguments, directory, echo: false);

    /// Saves all stdout/stderr to result's property <see cref="StartedProcess.Output"/> and prints them to Console.
    public static StartedProcess StartProcess_WithEcho
        (string file, string arguments, string directory = "") =>
        StartProcess_WithOutputHandler(file, arguments, directory, echo: true);

    public static StartedProcess StartProcess_WithOutputHandler
        (string file, string arguments, string directory, bool echo)
    {
#if DEBUG
        Log($"[RUN] >> {file} {arguments}", LogLevel.Debug, LogColor.Olive);
#endif
        var process = InitProcess(file, arguments, directory);
        var result = new StartedProcess(process);

        process.OutputDataReceived += (_, e) => result.SaveOutput(ProcessOutputKind.Output, e.Data, echo);
        process. ErrorDataReceived += (_, e) => result.SaveOutput(ProcessOutputKind.Error,  e.Data, echo);

        process.Start();

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        return result;
    }
}