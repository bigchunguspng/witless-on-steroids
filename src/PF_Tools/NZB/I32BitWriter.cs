using System.Text;

namespace PF_Tools.NZB;

public interface I32BitWriter : IDisposable
{
    public void WriteI32( int value);
    public void WriteU32(uint value);
}

public class BinaryWriter_NzbCompatible(Stream output) :
    BinaryWriter(output, Encoding.UTF8, leaveOpen: true),
    I32BitWriter
{
    public void WriteI32 (int value) => base.Write(value);
    public void WriteU32(uint value) => base.Write(value);
}