using System.Diagnostics;
using System.Text;

namespace PF_Tools.ProcessRunning;

public class StartedProcess(Process process)
{
    public Process       Process { get; } = process;
    public StringBuilder Output  { get; } = new();
}

public class ProcessResult(string arguments, StartedProcess process)
{
    public string        Arguments => arguments;
    public StringBuilder Output    => process.Output;
    public int           ExitCode  => process.Process.ExitCode;

    public bool WasKilled { get; set; }

    public bool Success => ExitCode == 0;
    public bool Failure => ExitCode != 0;
}

public class ProcessException(string executable, ProcessResult result) : Exception
{
    public string        File   { get; } = executable;
    public ProcessResult Result { get; } = result;
}