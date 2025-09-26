using System.Buffers.Binary;
using System.Text;

namespace PF_Tools.Backrooms.Helpers;

/// Non-Zero Bytes. Custom compression algorithm.
/// Best for an array of small positive int32's.
/// Stores data as
/// an array of 2-bit values (non-zero byte count)
/// and a chunk of those non-zero bytes.
public static class NZB
{
    /// Encodes a 4*count byte chunk of source stream as NZB data and writes it to target stream.
    // source / reader - memory stream: uncompressed data
    // target / writer - file   stream
    // uncompressed data is assumably written to source
    public static void Encode(Stream source, Stream target, int count)
    {
        // READ ints, WRITE zbls, nzbs
        var data_length = count * 4;
        var zbls_count = (count + 3) / 4;
        using var reader = new BinaryReader(source, Encoding.UTF8, leaveOpen: true);
        using var writer = new BinaryWriter(target, Encoding.UTF8, leaveOpen: true);
        using var ints_p = new PooledArray<uint>      (count);
        using var zbls_p = new PooledArray<byte> (zbls_count);

        var ints = ints_p.Array;
        var zbls = zbls_p.Array;
        var zbls_part = zbls.AsSpan(0, zbls_count);

        zbls_part.Fill(0); // who tf was the prev tenant

        // rewind reader (raw data, to compress) to data start
        reader.BaseStream.Seek(-data_length, SeekOrigin.Current);
        
        // read ints, gather zero-bytes lengths
        for (var i = 0; i < count; i++)
        {
            var u = ints[i] = reader.ReadUInt32();
            var bytesToSkip = u switch  //         RAW -> ZBLS + NZBS
            {                           //  0x00000000 -> 0b11 + 0x00......
                < 256             => 3, //  0xA1000000 -> 0b11 + 0xA1......
                < 265 * 256       => 2, //  0xA1A20000 -> 0b10 + 0xA1A2....
                < 256 * 256 * 256 => 1, //  0xA1A2A300 -> 0b01 + 0xA1A2A3..
                _                 => 0, //  0xA1A2A3A4 -> 0b00 + 0xA1A2A3A4
            };
            var qi = i >> 2;
            var shift = i & 3;
            zbls[qi] |= (byte)(bytesToSkip << 2 * shift);
        }

        // write zero-bytes lengths
        writer.Write(zbls_part);

        // write non-zero bytes
        var u32 = new byte[4];
        for (var i = 0; i < count; i++)
        {
            var qi = i >> 2;
            var shift = i & 3;
            var bytesToSkip = (zbls[qi] >> 2 * shift) & 3;
            BinaryPrimitives.WriteUInt32LittleEndian(u32, ints[i]);
            writer.Write(u32, 0, 4 - bytesToSkip);
        }
    }

    /// Decodes NZB data of a given count, from source stream and writes it to target stream.
    // source / reader - file   stream: compressed data
    // target / writer - memory stream
    // uncompressed data is assumably will be read from target
    public static void Decode(Stream source, Stream target, int count)
    {
        // READ zbls, nzbs, WRITE ints
        var data_length = count * 4;
        var zbls_count = (count + 3) / 4;
        using var reader = new BinaryReader(source, Encoding.UTF8, leaveOpen: true);
        using var writer = new BinaryWriter(target, Encoding.UTF8, leaveOpen: true);
        using var zbls_p = new PooledArray<byte> (zbls_count);

        var zbls = zbls_p.Array;
        var zbls_part = zbls.AsSpan(0, zbls_count);

        zbls_part.Fill(0); // who tf was the prev tenant

        // read zero-bytes lengths
        _ = reader.Read(zbls_part);

        // read non-zero bytes, restore them to ints
        var u32 = new byte[4];
        var u32_span = u32.AsSpan();
        for (var i = 0; i < count; i++)
        {
            var qi = i >> 2;   // i / 4
            var shift = i & 3; // i % 4
            var bytesToSkip = (zbls[qi] >> 2 * shift) & 3;

            u32_span.Fill(0);
            _ = reader.Read(u32, 0, 4 - bytesToSkip);
            writer.Write(u32);
        }

        // rewind writer (raw data, to be read) to data start
        writer.Seek(-data_length, SeekOrigin.Current);
    }
}