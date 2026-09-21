namespace PF_Tools.ProcessRunning;

public record struct ProcessOutputEntry(ProcessOutputKind Kind, string? Line);

public enum ProcessOutputKind { Output, Error, Other }

public class StartedProcess(Process process)
{
    public Process                  Process { get; } = process;
    public List<ProcessOutputEntry> Output  { get; } = [];

    public void SaveOutput
        (ProcessOutputKind kind, string? line, bool echo)
    {
        Output.Add(new ProcessOutputEntry(kind, line));
        if (echo) Console.WriteLine(line);
    }
}

public class ProcessResult(string arguments, StartedProcess process)
{
    public string                   Arguments => arguments;
    public List<ProcessOutputEntry> Output    => process.Output;
    public int                      ExitCode  => process.Process.ExitCode;

    public bool WasKilled { get; set; }

    public bool Success => ExitCode == 0;
    public bool Failure => ExitCode != 0;

    public string StdOut => string.Join('\n', Output.Where(x => x.Kind == ProcessOutputKind.Output).Select(x => x.Line));
    public string StdErr => string.Join('\n', Output.Where(x => x.Kind == ProcessOutputKind.Error ).Select(x => x.Line));

    public void AddComment
        (string line) => Output.Add(new ProcessOutputEntry(ProcessOutputKind.Other, line));
}

public class ProcessException(string executable, ProcessResult result) : Exception
{
    public string        File   { get; } = executable;
    public ProcessResult Result { get; } = result;
}