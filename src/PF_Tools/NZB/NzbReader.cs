using System.Buffers.Binary;
using System.Text;

namespace PF_Tools.NZB;

public struct NzbReader(Stream stream) : I32BitReader
{
    /// ZBCs = Zero Bytes Counts. Each byte stores 4 2-bit values.
    /// Each value stores a number (0-3) of trailing 0x00 bytes of a 32-bit item (LE).
    /// If all 4 bytes are 0x00, the value is still 3.
    /// So 1 byte here covers 4 32-bit items.
    /// Since data is stored in chunks of 1 - 65_536 items, this buffer is 16KB long.
    private readonly PooledArray<byte> _zbcs = new(NZB.ZBCS_BUFFER_LENGTH);
    //
    //
    private readonly BinaryReader    _reader = new(stream, Encoding.UTF8, leaveOpen: true);

    private int
        count = 0, // number of items in the current chunk, 0 - 65_536
        i     = 0; // current item index

    public  int ReadI32() => (int)Read_4B();
    public uint ReadU32() =>      Read_4B();

    private uint Read_4B()
    {
        if (i >= count) StartNewChunk();

        // get (n/-)ZBC
        var iQ    = i >> 2; // i / 4
        var shift = i  & 3; // i % 4
        var bytesToSkip = (_zbcs.Array[iQ] >> (shift << 1)) & 3;
        var bytesToRead = 4 - bytesToSkip;

        // read NZBs
        Span<byte>
            u32_LE = stackalloc byte[4],
            nzb    = u32_LE.Slice(0, bytesToRead);

        _ = _reader.Read(nzb);
        i++;

        return BinaryPrimitives.ReadUInt32LittleEndian(u32_LE);
    }

    private void StartNewChunk()
    {
        count = _reader.ReadUInt16() + 1; // 1 - 65_536
        _     = _reader.Read(_zbcs.Array, 0, (count + 3) >> 2);
        i     = 0;
    }

    public void Dispose()
    {
        _zbcs  .Dispose();
        _reader.Dispose();
    }
}