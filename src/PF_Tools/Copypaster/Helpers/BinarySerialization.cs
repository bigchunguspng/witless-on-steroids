using System.Text;
using PF_Tools.Copypaster.TransitionTables;
using PF_Tools.NZB;
using BinaryReader = System.IO.BinaryReader;

namespace PF_Tools.Copypaster.Helpers;

/// Serializes/deserializes <see cref="GenerationPack"/> into/from a binary file.
/// <code>
/// ┌─0B─────────────────────┬─4B─────────────────────┐
/// │ SpecialCount        4B │ OrdinalCount        4B │ # Headers
/// ├────────────────────────┼────────────────────────┤
/// ║ WordId                 ┊ Count              x8B ║ # Headers > Transitions tables / Special | 8B x    SpecialCount
/// ├────────────────────────┼────────────────────────┤
/// ║ Count #1           x4B │ Count #2               ║
/// ╟────────────────────────┼────────────────────────╢ # Headers > Transitions tables / ById    | 4B x VocabularyCount
/// ║ Count #N               │ *Padding*         0/4B ║
/// ├────────────────────────┼────────────────────────┤
/// ║ WordId                 ┊ Chance             x8B ║ ## Transitions
/// ├────────────────────────┴────────────────────────┤
/// ║ Null terminated UTF-8 strings                 ~ ║ ## Vocabulary
/// └─────────────────────────────────────────────────┘
/// </code>
public static class BinarySerialization
{
    // WRITE

    public static void Serialize(Stream output, GenerationPack pack, bool nzb = false)
    {
        using var writer = new BinaryWriter(output);

        writer.Write(pack.SpecialCount);
        writer.Write(pack.OrdinalCount);

        // Lengths (and names) of transition tables
        foreach (var pair in pack.TransitionsSpecial)
        {
            writer.Write(pair.Key);
            writer.Write(pair.Value.Count);
        }
        foreach (var table in pack.TransitionsOrdinal)
        {
            // todo, use nzb here too
            writer.Write(table.Count);
        }

        // Padding
        if (pack.OrdinalCount.IsOdd())
        {
            writer.Write(0xB0BA_0000);
        }

        // Transitions
        I32BitWriter writer_32b = nzb
            ? new    NzbWriter              (writer.BaseStream)
            : new BinaryWriter_NzbCompatible(writer.BaseStream);

        using (writer_32b) writer_32b.WriteTransitions(pack);

        // Vocabulary
        foreach (var word in pack.Vocabulary)
        {
            writer.Write(Encoding.UTF8.GetBytes(word));
            writer.Write((byte)0x00);
        }
    }

    private static void WriteTransitions(this I32BitWriter writer, GenerationPack pack)
    {
        writer.WriteTransitions(pack.TransitionsSpecial.SelectMany(x => x.Value.AsIEnumerable()));
        writer.WriteTransitions(pack.TransitionsOrdinal.SelectMany(x => x      .AsIEnumerable()));
    }

    private static void WriteTransitions(this I32BitWriter writer, IEnumerable<Transition> transitions)
    {
        foreach (var transition in transitions)
        {
            writer.WriteI32(transition.WordId);
            writer.WriteI32(transition.Chance);
        }
    }

    // READ

    public static GenerationPack Deserialize(Stream input, bool nzb = false)
    {
        using var reader = new BinaryReader(input);

        var countSpecial = reader.ReadInt32();
        var countOrdinal = reader.ReadInt32();

        // Lengths (and names) of transition tables
        var       transitionsSpecialCounts = new Dictionary<int, int>(countSpecial);
        using var transitionsOrdinalCounts = new PooledArray    <int>(countOrdinal);
        for (var i = 0; i < countSpecial; i++)
        {
            transitionsSpecialCounts.Add(reader.ReadInt32(), reader.ReadInt32());
        }
        for (var i = 0; i < countOrdinal; i++)
        {
            transitionsOrdinalCounts.Array[i] = reader.ReadInt32();
        }

        // Padding
        if (countOrdinal.IsOdd())
        {
            reader.ReadInt32();
        }

        // Transitions
        var transitionsSpecialTables = new Dictionary<int, TransitionTable>(countSpecial);
        var transitionsOrdinalTables = new List           <TransitionTable>(countOrdinal);

        using I32BitReader reader_32b = nzb
            ? new    NzbReader              (reader.BaseStream)
            : new BinaryReader_NzbCompatible(reader.BaseStream);
        reader_32b.ReadTransitionTables
        (
            transitionsSpecialCounts,
            transitionsOrdinalCounts.Array.AsSpan(0, countOrdinal),
            transitionsSpecialTables,
            transitionsOrdinalTables
        );

        // Vocabulary
        var vocabulary = reader.ReadVocabulary(countOrdinal);

        return new GenerationPack(vocabulary, transitionsOrdinalTables, transitionsSpecialTables);
    }

    private static void ReadTransitionTables
    (
        this I32BitReader reader,
        Dictionary<int, int>             transitionsSpecialCounts,
        Span           <int>             transitionsOrdinalCounts,
        Dictionary<int, TransitionTable> transitionsSpecialTables, 
        List           <TransitionTable> transitionsOrdinalTables
    )
    {
        foreach (var (wordId, count) in transitionsSpecialCounts)
        {
            var table = reader.ReadTransitions(count);
            transitionsSpecialTables.Add(wordId, table);
        }
        foreach (var          count  in transitionsOrdinalCounts)
        {
            var table = reader.ReadTransitions(count);
            transitionsOrdinalTables.Add(table);
        }
    }

    private static TransitionTable ReadTransitions(this I32BitReader reader, int count)
    {
        var transitions = new List<Transition>(count);
        for (var i = 0; i < count; i++)
        {
            transitions.Add(new Transition(reader.ReadI32(), reader.ReadI32()));
        }

        return GetTransitionTable(transitions);
    }

    private static TransitionTable GetTransitionTable(List<Transition> transitions) => transitions.Count switch
    {
           1 => new TransitionTableC1(transitions[0]),
           2 => new TransitionTableC2(transitions[0], transitions[1]),
        <= 8 => new TransitionTableV8(transitions),
        _    => new TransitionTableVU(transitions),
    };

    private static List<string> ReadVocabulary(this BinaryReader reader, int countVocabulary)
    {
        var wordLengths = new List<int>   (countVocabulary);
        var vocabulary  = new List<string>(countVocabulary);

        var position = reader.BaseStream.Position;
        {
            const int BUFFER_LENGTH = 1024;
            using var buffer = new PooledArray<byte>(BUFFER_LENGTH);
            var length = 0;
            while (true)
            {
                var bytesRead = reader.Read(buffer.Array, 0, BUFFER_LENGTH);
                for (var i = 0; i < bytesRead; i++)
                {
                    if (buffer.Array[i] == 0x00)
                    {
                        wordLengths.Add(length);
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
        reader.BaseStream.Position = position;

        foreach (var length in wordLengths)
        {
            var bytes = reader.ReadBytes(length);
            reader.ReadByte(); // skip null terminator
            vocabulary.Add(Encoding.UTF8.GetString(bytes));
        }

        return vocabulary;
    }
}