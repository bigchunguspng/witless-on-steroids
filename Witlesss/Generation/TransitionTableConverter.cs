using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Witlesss.Generation;

public class TransitionTableConverter : JsonConverter<Dictionary<int, TransitionTable>>
{
    public override void WriteJson(JsonWriter writer, Dictionary<int, TransitionTable> value, JsonSerializer serializer)
    {
        writer.WriteStartObject();

        foreach (var pair in value)
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
        JsonReader reader, Type type, Dictionary<int, TransitionTable> value, bool hasValue, JsonSerializer serializer
    )
    {
        var jo = JObject.Load(reader);
        var dictionary = new Dictionary<int, TransitionTable>(jo.Count);

        foreach (var property in jo.Properties())
        {
            var tableID = Base64Encoder.ToInt(property.Name);

            var transitionsJo = (JObject)property.Value;
            var transitions = new TransitionTable(transitionsJo.Count);

            foreach (var transition in transitionsJo)
            {
                var wordID = Base64Encoder.ToInt(transition.Key);
                var chance = transition.Value.ToObject<float>();
                transitions.Add(new Transition(wordID, chance));
            }

            dictionary.Add(tableID, transitions);
        }

        return dictionary;
    }
}