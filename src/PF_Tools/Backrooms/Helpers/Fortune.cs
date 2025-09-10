namespace PF_Tools.Backrooms.Helpers;

/// 🎲 I'm Feeling Lucky 🎲
public static class Fortune
{
    public static bool IsOneIn(int x)
        => Random.Shared.Next(x) == 0;

    public static bool IsFirstOf(int a, int b)
        => Random.Shared.Next(a + b) < a;

    public static bool LuckyFor(int chance, int max = 100)
        => Random.Shared.Next(max) < chance;

    public static int RandomInt(int min, int max)
        => Random.Shared.Next(min, max + 1);

    public static double RandomDouble(double min, double max)
    {
        var k = 10_000d;
        return RandomInt((int)(min * k), (int)(max * k)) / k;
    }
}