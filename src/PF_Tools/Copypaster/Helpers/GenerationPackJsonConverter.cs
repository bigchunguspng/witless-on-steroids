using System.Text.Json;
using System.Text.Json.Serialization;

namespace PF_Tools.Copypaster.Helpers;

public class GenerationPackJsonConverter : JsonConverter<GenerationPack>
{
    public override GenerationPack Read(ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
        => throw new NotImplementedException();

    public override void Write(Utf8JsonWriter writer, GenerationPack pack, JsonSerializerOptions options)
    {
        writer.WriteObject(() =>
        {
            writer.WriteNumber("count_special",    pack.SpecialCount);
            writer.WriteNumber("count_ordinal",    pack.OrdinalCount);
            writer.WriteNumber("count_vocabulary", pack.VocabularyCount);
            writer.WriteArray ("vocabulary", () => pack.Vocabulary.ForEach(writer.WriteStringValue));
            writer.WriteObject("transitions_special", () => pack.TransitionsSpecial.ForEach(WriteTransitionTable_Special));
            writer.WriteArray ("transitions_ordinal", () => pack.TransitionsOrdinal.ForEach(WriteTransitionTable_Ordinal));
        });

        void WriteTransitionTable_Special(KeyValuePair<int, TransitionTable> table)
            => writer.WriteObject
                (table.Key.ToString(), () => table.Value.AsIEnumerable().ForEach(WriteTransition));

        void WriteTransitionTable_Ordinal(                  TransitionTable  table)
            => writer.WriteObject
                (                      () => table      .AsIEnumerable().ForEach(WriteTransition));

        void WriteTransition(Transition transition)
            => writer.WriteNumber
                (transition.WordId.ToString(), transition.Chance);
    }
}