using System.Runtime.CompilerServices;

namespace PF_Tools.Logging;

public class FileLogger_Simple(string filePath)
{
    private readonly string?  _directory = Path.GetDirectoryName(filePath);

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Log(string message)
    {
        _directory.CreateDirectory();
        File.AppendAllText(filePath, message);
    }
}