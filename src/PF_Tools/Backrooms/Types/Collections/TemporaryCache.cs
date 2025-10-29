using System.Diagnostics.CodeAnalysis;

namespace PF_Tools.Backrooms.Types.Collections;

public class TemporaryCache<T>(TimeSpan retention)
{
    private T? Value;
    private DateTime EOL;

    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        value = Value;
        return value != null && EOL > DateTime.Now;
    }

    public bool TryGetValue_Failed([MaybeNullWhen(true)] out T value)
    {
        return TryGetValue(out value).Failed();
    }

    public void Set(T value)
    {
        Value = value;
        EOL = DateTime.Now + retention;
    }
}