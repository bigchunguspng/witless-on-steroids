using System.Diagnostics.CodeAnalysis;

namespace PF_Tools.Backrooms.Extensions;

public static class Extensions_Generic
{
    public static T Fluent<T>
        (this T obj, Action action)
    {
        action();
        return obj;
    }

    public static bool TryGetValue_Failed<TKey,TValue>
    (
        this Dictionary<TKey, TValue> dictionary,
        TKey key,
        [MaybeNullWhen(true)] out TValue value
    ) where TKey : notnull
    {
        return dictionary.TryGetValue(key, out value);
    }

    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
    {
        foreach (var element in source) action(element);
    }

    public static T GetRandomMemeber<T>() where T : struct, Enum
    {
        var values = Enum.GetValues<T>();
        return (T)values.GetValue(Random.Shared.Next(values.Length))!;
    }

    public static T PickAny<T>(this ICollection<T> collection)
    {
        return collection.ElementAt(Random.Shared.Next(collection.Count));
    }
}