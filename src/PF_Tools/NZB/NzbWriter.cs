using System.Buffers.Binary;
using System.Text;

namespace PF_Tools.NZB;

public struct NzbWriter(Stream stream) : I32BitWriter
{
    /// ZBCs = Zero Bytes Counts. Each byte stores 4 2-bit values.
    /// Each value stores a number (0-3) of trailing 0x00 bytes of a 32-bit item (LE).
    /// If all 4 bytes are 0x00, the value is still 3.
    /// So 1 byte here covers 4 32-bit items.
    /// Since data is stored in chunks of 1 - 65_536 items, this buffer is 16KB long.
    private readonly PooledArray<byte> _zbcs = new(NZB.ZBCS_BUFFER_LENGTH, clear: true);
    /// NZBs = Non-Zero Bytes.
    private readonly PooledArray<byte> _nzbs = new(NZB.NZBS_BUFFER_LENGTH);
    private readonly BinaryWriter    _writer = new(stream, Encoding.UTF8, leaveOpen: true);

    private int
        itemsWritten = 0, // number of items written to the current chunk buffer, 0 - 65_536
        bytesWritten = 0; // number of bytes written to the current chunk buffer

    public void WriteI32 (int value) => Write_4B((uint)value);
    public void WriteU32(uint value) => Write_4B      (value);

    private void Write_4B(uint u32)
    {
        // get (n/-)ZBC
        var bytesToSkip = GetZeroBytesCount(u32);
        var bytesToWrite = 4 - bytesToSkip;

        // write ZBC
        var iQ    = itemsWritten >> 2; // i / 4
        var shift = itemsWritten  & 3; // i % 4

        _zbcs.Array[iQ] |= (byte)(bytesToSkip << (shift << 1));

        // write NZBs
        Span<byte>
            u32_LE = stackalloc byte[4],
            source =                  u32_LE.Slice(0, bytesToWrite),
            target = _nzbs.Array.AsSpan(bytesWritten, bytesToWrite);

        BinaryPrimitives.WriteUInt32LittleEndian(u32_LE, u32);
        source.CopyTo(target);

        //
        itemsWritten++;
        bytesWritten += bytesToWrite;

        if (itemsWritten >= NZB.CHUNK_ITEMS_MAX)
        {
            FlushChunk();
            Reset();
        }
    }

    private int GetZeroBytesCount(uint u32) => u32 switch
    {                            //  0x00000000 -> 0b11 + 0x00......
        < 256             => 3,  //  0xA1000000 -> 0b11 + 0xA1......
        < 256 * 256       => 2,  //  0xA1A20000 -> 0b10 + 0xA1A2....
        < 256 * 256 * 256 => 1,  //  0xA1A2A300 -> 0b01 + 0xA1A2A3..
        _                 => 0,  //  0xA1A2A3A4 -> 0b00 + 0xA1A2A3A4
    };                           //         RAW -> ZBCS + NZBS

    private void FlushChunk()
    {
        _writer.Write((ushort)(itemsWritten - 1)); // 0 - 65_535
        _writer.Write(_zbcs.Array, 0, (itemsWritten + 3) >> 2);
        _writer.Write(_nzbs.Array, 0, bytesWritten);
    }

    private void Reset()
    {
        itemsWritten = bytesWritten = 0;
        _zbcs.Array.AsSpan(0, NZB.ZBCS_BUFFER_LENGTH).Clear();
    }

    public void Dispose()
    {
        if (itemsWritten > 0) FlushChunk();

        _zbcs  .Dispose();
        _nzbs  .Dispose();
        _writer.Dispose();
    }
}