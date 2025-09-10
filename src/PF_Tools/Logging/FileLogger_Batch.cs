using System.Runtime.CompilerServices;

namespace PF_Tools.Logging;

/// Stores logs and write them to a file in batches (hardcoded to 32).
public class FileLogger_Batch(FilePath filePath)
{
    private readonly string?  _directory = filePath.DirectoryName;
    private readonly string[] _buffer    = new string[32];

    private int _head;

    /// Logs message. Triggers write every 32 calls.
    [MethodImpl(MethodImplOptions.Synchronized)]
    public void Log(string message)
    {
        _buffer[_head] = message;
        MoveHead();
    }

    private void MoveHead()
    {
        if (++_head < 32) return;

        Write();
        _head &= 0b00011111; // same as %= 32
    }

    /// Writes pending logs to file. Call this before exit!
    public void Write()
    {
        _directory.CreateDirectory();
        File.AppendAllLines(filePath, _buffer.Take(_head));
    }
}