using System.Text.Json;
using System.Text.Json.Serialization;
using PF_Bot.Routing.Messages;
using Telegram.Bot.Types;

namespace PF_Bot.Routing.Callbacks;

public class CallbackContext
    (CallbackQuery query, string key, string content)
    : MessageContext(query.Message!)
{
    public CallbackQuery Query   { get; } = query;
    public string        Key     { get; } = key;
    public string        Content { get; } = content;
}

public class CallbackContextJsonConverter : JsonConverter<CallbackContext>
{
    public override CallbackContext Read
        (ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
        => throw new NotImplementedException();

    public override void Write
    (
        Utf8JsonWriter writer,
        CallbackContext value,
        JsonSerializerOptions options
    ) => writer.WriteObject(() =>
    {
        writer.WriteObject("message", value.Query, options);
        writer.WriteString("title", value.Title);
        writer.WriteNumber("chat", value.Chat);
        writer.WriteString("key", value.Key);
        writer.WriteString("content", value.Content);
    });
}
