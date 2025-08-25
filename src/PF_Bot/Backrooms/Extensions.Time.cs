using Stopwatch = System.Diagnostics.Stopwatch;

namespace PF_Bot.Backrooms;

public static partial class Extensions
{
    public static Stopwatch GetStartedStopwatch()
    {
        var sw = new Stopwatch();
        sw.Start();
        return sw;
    }

    public static void Log(this Stopwatch sw, string message)
    {
        Print($" [  T  I  M  E  ]  -  {sw.ElapsedExact()} {message}", ConsoleColor.DarkGray);
        sw.Restart();
    }

    public static string ElapsedShort(this Stopwatch sw) => sw.Elapsed.ReadableTimeShort();
    public static string ElapsedExact(this Stopwatch sw) => sw.Elapsed.ReadableTimeExact();

    public static T MeasureTime<T>(Func<T> func, string caption)
    {
        var sw = GetStartedStopwatch();
        var result = func.Invoke();
        sw.Log(caption);
        return result;
    }

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