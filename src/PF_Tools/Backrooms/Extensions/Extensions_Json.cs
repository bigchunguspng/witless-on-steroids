using System.Text.Json;

namespace PF_Tools.Backrooms.Extensions;

public static class Extensions_Json
{
    public static void WriteArray(this Utf8JsonWriter writer, Action writeBody)
    {
        writer.WriteStartArray();
        writeBody();
        writer.WriteEndArray();
    }

    public static void WriteObject(this Utf8JsonWriter writer, Action writeBody)
    {
        writer.WriteStartObject();
        writeBody();
        writer.WriteEndObject();
    }

    public static void WriteArray(this Utf8JsonWriter writer, string name, Action writeBody)
    {
        writer.WriteStartArray(name);
        writeBody();
        writer.WriteEndArray();
    }

    public static void WriteObject(this Utf8JsonWriter writer, string name, Action writeBody)
    {
        writer.WriteStartObject(name);
        writeBody();
        writer.WriteEndObject();
    }
}