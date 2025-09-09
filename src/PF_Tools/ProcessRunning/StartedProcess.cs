using System.Diagnostics;
using System.Text;

namespace PF_Tools.ProcessRunning;

public class StartedProcess(Process process)
{
    public Process       Process { get; } = process;
    public StringBuilder Output  { get; } = new();
}