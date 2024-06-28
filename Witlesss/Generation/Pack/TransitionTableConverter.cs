using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Witlesss.Backrooms.Helpers;

namespace Witlesss.Generation.Pack;

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

            foreach (var transition in pair.Value)
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
        var depth = 0;

        TransitionTable transitions = null!;

        while (reader.Read())
        {
            if /**/ (reader.TokenType == JsonToken.StartObject) depth++;
            else if (reader.TokenType == JsonToken.  EndObject) depth--;
            else
            {
                if (reader.TokenType == JsonToken.PropertyName && reader.ValueType == typeof(string))
                {
                    var id = Base64Encoder.ToInt((reader.Value as string)!);

                    if (depth == 0) // keys
                    {
                        transitions = [];
                        dictionary.Add(id, transitions);
                    }
                    else if (depth == 1) // values
                    {
                        var chance = (float)reader.ReadAsDouble()!;
                        transitions.Add(new Transition(id, chance));
                    }
                }
            }

            if (depth < 0) return dictionary;
        }

        return dictionary;
    }
}