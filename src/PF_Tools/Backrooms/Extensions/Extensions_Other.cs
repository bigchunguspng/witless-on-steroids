namespace PF_Tools.Backrooms.Extensions;

public static class Extensions_Other
{
    public static T PickAny<T>(this ICollection<T> collection)
    {
        return collection.ElementAt(Random.Shared.Next(collection.Count));
    }
}