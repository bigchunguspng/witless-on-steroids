using Stopwatch = System.Diagnostics.Stopwatch;

namespace Witlesss.Backrooms;

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
        Logger.Log($"{sw.Elapsed.TotalSeconds:##0.00000}\t{message}");
        sw.Restart();
    }

    public static T MeasureTime<T>(Func<T> func, string caption)
    {
        var sw = GetStartedStopwatch();
        var result = func.Invoke();
        sw.Log(caption);
        return result;
    }
}