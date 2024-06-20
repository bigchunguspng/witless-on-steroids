using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Witlesss.Generation;

public class TransitionTableConverter : JsonConverter<TransitionTable>
{
    public override void WriteJson(JsonWriter writer, TransitionTable value, JsonSerializer serializer)
    {
        writer.WriteStartObject();

        foreach (var transition in value)
        {
            writer.WritePropertyName(Base64Encoder.ToString(transition.WordID));
            writer.WriteValue(transition.Chance);
        }

        writer.WriteEndObject();
    }

    public override TransitionTable ReadJson
    (
        JsonReader reader, Type type, TransitionTable value, bool hasvalue, JsonSerializer serializer
    )
    {
        var jo = JObject.Load(reader);
        var transitions = new TransitionTable();

        foreach (var property in jo.Properties())
        {
            var wordID = Base64Encoder.ToInt(property.Name);
            var chance = property.Value.ToObject<float>();
            transitions.Add(new Transition(wordID, chance));
        }

        return transitions;
    }
}