namespace PF_Tools.Backrooms.Extensions;

public static class Extensions_Time
{
    public static void Log(this Stopwatch sw, string message)
    {
        Print($" [TIME] 0x{sw.GetHashCode():x8} * {sw.ElapsedReadable(),10} -> {message}", ConsoleColor.DarkGray);
        sw.Restart();
    }

    public static string ElapsedReadable
        (this Stopwatch sw) => sw.Elapsed.ReadableTime();

    public static string ReadableTime // max length - 10 chars
        (this TimeSpan t)
        =>    t.TotalSeconds <  10 ? $@"{t:s\.fff}'{t.Microseconds/10:00} s"
            : t.TotalMinutes <   1 ? $@"{t:s\.fff' s'}"
            : t.TotalHours   <   1 ? $@"{t:m\:ss' m'}"
            : t.TotalDays    <   1 ? $@"{t:h\:mm' h'}"
            : t.TotalDays    < 100 ? $@"{t:d\.hh\:mm' d'}"
            :                         $"{t.TotalDays:F1} d";

    public static bool HappenedWithinLast
        (this DateTime date, TimeSpan span) => DateTime.Now - date < span;
}

public static class TimeMath
{
    public static TimeSpan Min(TimeSpan a, TimeSpan b) => a.Ticks < b.Ticks ? a : b;
    public static TimeSpan Max(TimeSpan a, TimeSpan b) => a.Ticks > b.Ticks ? a : b;
}