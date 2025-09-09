using System.Text;

namespace PF_Tools.ProcessRunning;

public class ProcessResult(string arguments, StartedProcess process)
{
    public string        Arguments { get; } = arguments;
    public StringBuilder Output    { get; } = process.Output;
    public int           ExitCode  { get; } = process.Process.ExitCode;

    public bool WasKilled { get; set; }

    public bool Success => ExitCode == 0;
    public bool Failure => ExitCode != 0;
}

public class ProcessException(string executable, ProcessResult result) : Exception
{
    public string        File   { get; } = executable;
    public ProcessResult Result { get; } = result;
}