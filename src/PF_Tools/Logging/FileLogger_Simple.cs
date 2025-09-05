using System.Runtime.CompilerServices;
using PF_Tools.Backrooms.Types;

namespace PF_Tools.Logging;

public class FileLogger_Simple(FilePath filePath)
{
    private readonly string?  _directory = filePath.DirectoryName;

    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Log(string message)
    {
        _directory.CreateDirectory();
        File.AppendAllText(filePath, message);
    }
}