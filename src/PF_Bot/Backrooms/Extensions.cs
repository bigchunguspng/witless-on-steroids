using System.Globalization;

namespace PF_Bot.Backrooms;

public static partial class Extensions
{
    //

    public static string HOURS_ED(int hours) => hours.ED( "", "а", "ов");
    public static string  MINS_ED(int mins ) => mins. ED("у", "ы",   "");

    public static string ED(this int x, string x1, string x234, string xAny) =>
        x is 0
          or >= 5 and <= 20
            ? xAny
            : (x % 10) switch
            {
                1   => x1,
                < 5 => x234,
                _   => xAny,
            };

    //

    public static string GetErrorMessage
        (this Exception e) => e is AggregateException a 
        ? a.InnerException!.GetErrorMessage_Internal() 
        : e                .GetErrorMessage_Internal();

    private static string GetErrorMessage_Internal
        (this Exception e) => $"{e.GetType().Name} >> {e.Message}\n{(e.StackTrace ?? "").SubstringTill('\n')}";

    // UI

    public static string ReadableFileSize
        (this long bytes)
    {
        var kbs = bytes / 1024F;
        var mbs = kbs   / 1024F;
        return mbs >= 100 ? UI($"{mbs:F1} МБ")
            :  mbs >=   1 ? UI($"{mbs:F2} МБ")
            :  kbs >=   1 ? UI($"{kbs:F0} КБ")
            :                  $"{bytes} байт";
    }

    public static string Format_bruh_1k_100k_1M
        (this int x, string bruh = "💀") => x switch
    {
        < 1000      =>        x + bruh,
        < 100_000   => UI($@"{x / 1000D:0.#}k👌"),
        < 1_000_000 =>    $@"{x / 1000}k👌",
        _           =>    $@"{x / 1_000_000}M 🤯",
    };

    public static readonly CultureInfo Culture_UI = CultureInfo.GetCultureInfo("fi"/*nna jerk it))*/);

    public static string UI
        (this FormattableString formattable) => formattable.ToString(Culture_UI);
}