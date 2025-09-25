using System.Text;
using PF_Tools.Copypaster.TransitionTables;

namespace PF_Tools.Copypaster.Helpers;

/// Serializes/deserializes <see cref="GenerationPack"/> into/from a binary file.
/// <code>
/// ┌─0B─────────────────────┬─4B─────────────────────┐
/// │ SpecialCount        4B │ VocabularyCount     4B │ # Headers
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
    public static void Serialize(BinaryWriter writer, GenerationPack pack)
    {
        writer.Write(pack.SpecialCount);
        writer.Write(pack.VocabularyCount);

        // Lengths (and names) of transition tables
        foreach (var pair in pack.TransitionsSpecial)
        {
            writer.Write(pair.Key);
            writer.Write(pair.Value.Count);
        }
        foreach (var table in pack.TransitionsById)
        {
            writer.Write(table.Count);
        }

        // Padding
        if (pack.VocabularyCount.IsOdd())
        {
            writer.Write(0xB0BA_0000);
        }

        // Transitions
        foreach (var transition in pack.TransitionsSpecial.SelectMany(x => x.Value.AsIEnumerable()))
        {
            writer.Write(transition.WordId);
            writer.Write(transition.Chance);
        }
        foreach (var transition in pack.TransitionsById.SelectMany(x => x.AsIEnumerable()))
        {
            writer.Write(transition.WordId);
            writer.Write(transition.Chance);
        }

        // Vocabulary
        foreach (var word in pack.Vocabulary)
        {
            writer.Write(Encoding.UTF8.GetBytes(word));
            writer.Write((byte)0x00);
        }
    }

    public static GenerationPack Deserialize(BinaryReader reader)
    {
        var countSpecial    = reader.ReadInt32();
        var countVocabulary = reader.ReadInt32();

        // Lengths (and names) of transition tables
        var transitionsSpecialCounts = new Dictionary<int, int>(countSpecial);
        var transitionsByIdCounts = new List<int>(countVocabulary);
        for (var i = 0; i < countSpecial; i++)
        {
            transitionsSpecialCounts.Add(reader.ReadInt32(), reader.ReadInt32());
        }
        for (var i = 0; i < countVocabulary; i++)
        {
            transitionsByIdCounts.Add(reader.ReadInt32());
        }

        // Padding
        if (countVocabulary.IsOdd())
        {
            reader.ReadInt32();
        }

        // Transitions
        var transitionsSpecial = new Dictionary<int, TransitionTable>(countSpecial);
        var transitionsById = new List<TransitionTable>(countVocabulary);
        foreach (var pair in transitionsSpecialCounts)
        {
            var count = pair.Value;
            var transitions = new List<Transition>(count);
            for (var i = 0; i < count; i++)
            {
                transitions.Add(new Transition(reader.ReadInt32(), reader.ReadInt32()));
            }

            transitionsSpecial.Add(pair.Key, GetTransitionTable(transitions));
        }
        foreach (var count in transitionsByIdCounts)
        {
            var transitions = new List<Transition>(count);
            for (var i = 0; i < count; i++)
            {
                transitions.Add(new Transition(reader.ReadInt32(), reader.ReadInt32()));
            }

            transitionsById.Add(GetTransitionTable(transitions));
        }

        // Vocabulary
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

        return new GenerationPack(vocabulary, transitionsById, transitionsSpecial);
    }

    private static TransitionTable GetTransitionTable(List<Transition> transitions) => transitions.Count switch
    {
           1 => new TransitionTableC1(transitions[0]),
           2 => new TransitionTableC2(transitions[0], transitions[1]),
        <= 8 => new TransitionTableV8(transitions),
        _    => new TransitionTableVU(transitions)
    };
}