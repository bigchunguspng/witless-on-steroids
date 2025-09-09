using System.Diagnostics;

namespace PF_Tools.Backrooms.Extensions;

public static class Extensions_Time
{
    public static T MeasureTime<T>(Func<T> func, string caption)
    {
        var sw = Stopwatch_StartNew();
        var result = func.Invoke();
        sw.Log(caption);
        return result;
    }

    public static Stopwatch Stopwatch_StartNew()
    {
        return Stopwatch.StartNew();
    }

    public static void Log(this Stopwatch sw, string message)
    {
        Print($" [  T  I  M  E  ]  -  {sw.ElapsedReadable()} -> {message}", ConsoleColor.DarkGray);
        sw.Restart();
    }

    public static string ElapsedReadable
        (this Stopwatch sw) => sw.Elapsed.ReadableTime();

    public static string ReadableTime
        (this TimeSpan t)
        =>    t.TotalSeconds < 10 ? $@"{t:s\,fff'`'ff' s'}"
            : t.TotalMinutes <  1 ? $@"{t:s\,fff' s'}"
            : t.TotalMinutes <  2 ? $"{t:m' MIN 's' s'}"
            : t.TotalMinutes <  5 ? $"{t:m' MINS 's' s'}"
            : t.TotalHours   <  1 ? $"{t:m' MINS'}"
            : t.TotalHours   <  2 ? $"{t:h' HOUR 'm' MINS'}"
            : t.TotalHours   <  5 ? $"{t:h' HOURS 'm' MINS'}"
            : t.TotalDays    <  1 ? $"{t:h' HOURS'}"
            : t.TotalDays    <  2 ? $"{t:d' DAY 'h' HOURS'}"
            : t.TotalDays    <  5 ? $"{t:d' DAYS 'h' HOURS'}"
            :                       $"{t:d' DAYS'}";

    public static bool HappenedWithinLast
        (this DateTime date, TimeSpan span) => DateTime.Now - date < span;
}