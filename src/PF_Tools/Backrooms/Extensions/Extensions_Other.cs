namespace PF_Tools.Backrooms.Extensions;

public static class Extensions_Other
{
    public static T Fluent<T>
        (this T obj, Action action)
    {
        action();
        return obj;
    }

    public static T PickAny<T>(this ICollection<T> collection)
    {
        return collection.ElementAt(Random.Shared.Next(collection.Count));
    }
}