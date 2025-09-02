using System.Runtime.CompilerServices;

namespace PF_Tools.Logging;

public class FileLogger_Simple(string filePath)
{
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Log(string message)
    {
        filePath.CreateParentDirectory();
        File.AppendAllText(filePath, message);
    }
}