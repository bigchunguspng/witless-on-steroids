using System.Text.Json;
using System.Text.Json.Serialization;

namespace PF_Tools.Backrooms.Types;

public abstract class WriteOnlyJsonConverter<T> : JsonConverter<T>
{
    public sealed override T Read
        (ref Utf8JsonReader reader, Type type, JsonSerializerOptions options)
        => throw new NotImplementedException();
}