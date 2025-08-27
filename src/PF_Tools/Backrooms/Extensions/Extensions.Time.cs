using Stopwatch = System.Diagnostics.Stopwatch;

namespace PF_Tools.Backrooms.Extensions;

public static partial class Extensions
{
    public static Stopwatch GetStartedStopwatch()
    {
        var sw = new Stopwatch();
        sw.Start();
        return sw;
    }

    public static string ElapsedShort(this Stopwatch sw) => sw.Elapsed.ReadableTimeShort();
    public static string ElapsedExact(this Stopwatch sw) => sw.Elapsed.ReadableTimeExact();

    public static string ReadableTimeShort
        (this TimeSpan t) => t.Minutes >= 5
        ? $"{t:m' MINS'}"
        : t.Minutes >= 1
            ? $@"{t:m' MIN 's' s'}"
            : $@"{t:s\,fff' s'}";

    public static string ReadableTimeExact
        (this TimeSpan t) => t.Minutes >= 5
        ? $"{t:m' MINS'}"
        : t.Minutes >= 1
            ? $@"{t:m' MIN 's' s'}"
            : t.Seconds >= 10
                ? $@"{t:s\,fff}"
                : $@"{t:s\,fffff}";

    public static bool HappenedWithinLast
        (this DateTime date, TimeSpan span) => DateTime.Now - date < span;
}