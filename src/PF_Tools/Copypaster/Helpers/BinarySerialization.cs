using System.Text;
using PF_Tools.Copypaster.TransitionTables;
using PF_Tools.NZB;
using BinaryReader = System.IO.BinaryReader;

namespace PF_Tools.Copypaster.Helpers;

/// Serializes/deserializes <see cref="GenerationPack"/> into/from a binary file.
/// <code>
///   ┌─0B─────────────────────┬─4B─────────────────────┐
///   │ SpecialCount        4B │ OrdinalCount        4B │ # Headers
/// ╒ ├────────────────────────┼────────────────────────┤
/// ░ ║ WordId                 ┊ Count              x8B ║ # Headers > Transitions tables / Special | 8B x    SpecialCount
/// ░ ├────────────────────────┼────────────────────────┤
/// N ║ Count #1           x4B │ Count #2               ║
/// Z ╟────────────────────────┼────────────────────────╢ # Headers > Transitions tables / ById    | 4B x VocabularyCount
/// B ║ Count #N               │ *Padding*         0/4B ║
/// ░ ├────────────────────────┼────────────────────────┤
/// ░ ║ WordId                 ┊ Chance             x8B ║ ## Transitions
/// ╘ ├────────────────────────┴────────────────────────┤
///   ║ Null terminated UTF-8 strings                 ~ ║ ## Vocabulary
///   └─────────────────────────────────────────────────┘
/// </code>
public static class BinarySerialization
{
    // //// ///// //// //
    // //// WRITE //// //

    public static void Serialize(Stream output, GenerationPack pack, bool nzb = false)
    {
        using var writer = new BinaryWriter(output, Encoding.UTF8, leaveOpen: true);
        var       writer_32b = output.GetNzbCompatibleWriter(nzb);

        // Headers
        writer.Write(pack.SpecialCount);
        writer.Write(pack.OrdinalCount);

        // Transitions
        using (writer_32b) writer_32b.WriteTransitionsData(pack, nzb);

        // Vocabulary
        foreach (var word in pack.Vocabulary)
        {
            writer.Write(Encoding.UTF8.GetBytes(word));
            writer.Write((byte)0x00);
        }
    }

    private static void WriteTransitionsData(this I32BitWriter writer, GenerationPack pack, bool nzb)
    {
        // Transition tables lengths
        foreach (var (wordId, table) in pack.TransitionsSpecial)
        {
            writer.WriteI32(wordId);
            writer.WriteI32(table.Count);
        }
        foreach (var          table  in pack.TransitionsOrdinal)
        {
            writer.WriteI32(table.Count);
        }

        // Padding
        if (nzb.Janai() && pack.OrdinalCount.IsOdd())
        {
            writer.WriteU32(0xB0BA_0000);
        }

        // Transitions
        foreach (var (_, table) in pack.TransitionsSpecial)
        foreach (var transition in table.AsIEnumerable())
        {
            writer.WriteTransition(transition);
        }
        foreach (var     table  in pack.TransitionsOrdinal)
        foreach (var transition in table.AsIEnumerable())
        {
            writer.WriteTransition(transition);
        }
    }

    // //// //// //// //
    // //// READ //// //

    public static GenerationPack Deserialize(Stream input, bool nzb = false)
    {
        using var reader = new BinaryReader(input, Encoding.UTF8, leaveOpen: true);
        using var reader_32b = input.GetNzbCompatibleReader(nzb);
        
        var countSpecial = reader.ReadInt32();
        var countOrdinal = reader.ReadInt32();

        var extraCapacity = Math.Clamp(countOrdinal >> 8, 4, 20);

        var transitionsSpecialTables = new Dictionary<int, TransitionTable>(countSpecial);
        var transitionsOrdinalTables = new List           <TransitionTable>(countOrdinal + extraCapacity);
        var vocabulary               = new List                    <string>(countOrdinal + extraCapacity);

        var       transitionsSpecialCounts = new Dictionary<int, int>(countSpecial);
        using var transitionsOrdinalCounts = new PooledArray    <int>(countOrdinal);
        using var wordLengths              = new PooledArray    <int>(countOrdinal);

        // Transition tables lengths
        for (var i = 0; i < countSpecial; i++)
        {
            transitionsSpecialCounts.Add(reader_32b.ReadI32(), reader_32b.ReadI32());
        }
        for (var i = 0; i < countOrdinal; i++)
        {
            transitionsOrdinalCounts.Array[i] = reader_32b.ReadI32();
        }

        // Padding
        if (!nzb && countOrdinal.IsOdd())
        {
            reader_32b.ReadI32();
        }

        // Transitions
        foreach (var (wordId, count) in transitionsSpecialCounts)
        {
            var table = reader_32b.ReadTransitionTable(count);
            transitionsSpecialTables.Add(wordId, table);
        }
        foreach (var          count  in transitionsOrdinalCounts.Array.Slice(countOrdinal))
        {
            var table = reader_32b.ReadTransitionTable(count);
            transitionsOrdinalTables.Add(table);
        }

        // Vocabulary
        reader.ReadVocabulary(wordLengths.Array.Slice(countOrdinal), vocabulary);

        return new GenerationPack(vocabulary, transitionsOrdinalTables, transitionsSpecialTables);
    }

    private static TransitionTable ReadTransitionTable(this I32BitReader reader, int count) => count switch
    {
        1 => new TransitionTableC1(reader.ReadTransition()),
        2 => new TransitionTableC2(reader.ReadTransition(), reader.ReadTransition()),
        _ => ReadTransitionTableV_(reader, count),
    };

    private static TransitionTable ReadTransitionTableV_(I32BitReader reader, int count)
    {
        var transitions = new Transition[count];
        for (var i = 0; i < count; i++)
        {
            transitions[i] = reader.ReadTransition();
        }

        return count <= 8 
            ? new TransitionTableV8(transitions) 
            : new TransitionTableVU(transitions);
    }

    private static void ReadVocabulary(this BinaryReader reader, Span<int> wordLengths, List<string> vocabulary)
    {
        var bonfire = reader.BaseStream.Position;
        reader.ReadStringLengths(wordLengths, out var lengthMax);
        reader.BaseStream.Position = bonfire;
        reader.ReadStrings(vocabulary, wordLengths, lengthMax);
    }

    private static void ReadStringLengths(this BinaryReader reader, Span<int> stringLengths, out int lengthMax)
    {
        const int BUFFER_LENGTH = 16 * 1024;

        lengthMax = 0;

        int index = 0, length = 0;

        using var buffer = new PooledArray<byte>(BUFFER_LENGTH);
        while (true)
        {
            var bytesRead = reader.Read(buffer.Array, 0, BUFFER_LENGTH);
            for (var i = 0; i < bytesRead; i++)
            {
                if (buffer.Array[i] == 0x00)
                {
                    stringLengths[index++] = length;
                    lengthMax = Math.Max(lengthMax, length);
                    length = 0;
                }
                else
                {
                    length++;
                }
            }

            if (bytesRead < BUFFER_LENGTH) break;
        }
    }

    private static void ReadStrings(this BinaryReader reader, List<string> strings, Span<int> stringLengths, int lengthMax)
    {
        using var buffer = new PooledArray<byte>(lengthMax + 1);

        foreach (var length in stringLengths)
        {
            _ = reader.Read // (null terminated)
                (buffer.Array, 0, length + 1);
            strings.Add(Encoding.UTF8.GetString
                (buffer.Array, 0, length));
        }
    }

    // ...

    private static void
        WriteTransition
        (this I32BitWriter writer, Transition transition)
    {
        writer.WriteI32(transition.WordId);
        writer.WriteI32(transition.Chance);
    }

    private static Transition
        ReadTransition
        (this I32BitReader reader) => new(reader.ReadI32(), reader.ReadI32());


    private static I32BitReader GetNzbCompatibleReader
        (this Stream stream, bool nzb) => nzb
        ? new    NzbReader              (stream)
        : new BinaryReader_NzbCompatible(stream);

    private static I32BitWriter GetNzbCompatibleWriter
        (this Stream stream, bool nzb) => nzb
        ? new    NzbWriter              (stream)
        : new BinaryWriter_NzbCompatible(stream);
}