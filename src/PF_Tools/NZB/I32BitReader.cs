using System.Text;

namespace PF_Tools.NZB;

public interface I32BitReader : IDisposable
{
    public  int ReadI32();
    public uint ReadU32();
}

public class BinaryReader_NzbCompatible(Stream input) : 
    BinaryReader(input, Encoding.UTF8, leaveOpen: true), 
    I32BitReader
{
    public  int ReadI32() => base.ReadInt32();
    public uint ReadU32() => base.ReadUInt32();
}