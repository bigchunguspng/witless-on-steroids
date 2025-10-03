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

    public static async Task<(string stdout, string stderr)> Run_GetOutput
        (string cmd, string args, string directory = "")
    {
        var   process = ProcessStarter.InitProcess(cmd, args, directory);
              process.Start();
        await process.WaitForExitAsync();
        return
        (
            await process.StandardOutput.ReadToEndAsync(),
            await process.StandardError .ReadToEndAsync()
        );
    }
}