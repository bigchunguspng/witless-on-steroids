using Newtonsoft.Json;

namespace PF_Tools.Copypaster_Legacy.Pack;

public class TransitionTableConverter : JsonConverter<Dictionary<int, TransitionTable>>
{
    public override void WriteJson
    (
        JsonWriter writer,
        Dictionary<int, TransitionTable> dictionary,
        JsonSerializer serializer
    )
    {
        writer.WriteStartObject();

        foreach (var pair in dictionary)
        {
            writer.WritePropertyName(Base64Encoder.ToString(pair.Key));
            writer.WriteStartObject();

            foreach (var transition in pair.Value.AsIEnumerable())
            {
                writer.WritePropertyName(Base64Encoder.ToString(transition.WordID));
                writer.WriteValue(transition.Chance);
            }

            writer.WriteEndObject();
        }

        writer.WriteEndObject();
    }

    public override Dictionary<int, TransitionTable> ReadJson
    (
        JsonReader reader,
        Type type,
        Dictionary<int, TransitionTable> dictionary,
        bool hasValue,
        JsonSerializer serializer
    )
    {
        int depth = 0, id = 0;

        TransitionTable table = null!;

        while (reader.Read())
        {
            if /**/ (reader.TokenType == JsonToken.StartObject) depth++;
            else if (reader.TokenType == JsonToken.  EndObject) depth--;
            else if (reader.TokenType == JsonToken.PropertyName && reader.ValueType == typeof(string))
            {
                if (depth == 0) // keys
                {
                    id = ReadID();
                    table = new TransitionTableSingle();
                    dictionary.Add(id, table);
                }
                else if (depth == 1) // values
                {
                    var wordID = ReadID();
                    var chance = (float)reader.ReadAsDouble()!;

                    if (!table.CanAccept(wordID))
                    {
                        TransitionTable.Upgrade(ref table);
                        dictionary[id] = table;
                    }

                    table.Add(new Transition(wordID, chance));
                }
            }

            if (depth < 0) return dictionary;
        }

        return dictionary;

        int ReadID() => Base64Encoder.ToInt((reader.Value as string)!);
    }
}