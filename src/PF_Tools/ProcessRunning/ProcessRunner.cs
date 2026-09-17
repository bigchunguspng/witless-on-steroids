namespace PF_Tools.ProcessRunning;

public static class ProcessRunner
{
    public static async Task<ProcessResult> Run
        (string file, string arguments, string directory = "")
    {
        var startedProcess = ProcessStarter.StartProcess
            (file, arguments, directory);
        await startedProcess.Process.WaitForExitAsync();
        return new ProcessResult(arguments, startedProcess);
    }

    public static async Task<ProcessResult> Run_WithEcho
        (string file, string arguments, string directory = "")
    {
        var startedProcess = ProcessStarter.StartProcess_WithEcho
            (file, arguments, directory);
        await startedProcess.Process.WaitForExitAsync();
        return new ProcessResult(arguments, startedProcess);
    }

    public static async Task<(string stdout, string stderr, int code)> Run_GetOutput
        (string cmd, string args, string directory = "")
    {
        var process = ProcessStarter.InitProcess(cmd, args, directory);
        process.Start();

        var read_stdout = process.StandardOutput.ReadToEndAsync();
        var read_stderr = process.StandardError .ReadToEndAsync();
        // ^ launch reading tasks before waiting!
        //   otherwise it won't work when output's too long

        await process.WaitForExitAsync();
        return
        (
            await read_stdout,
            await read_stderr,
            process.ExitCode
        );
    }
}