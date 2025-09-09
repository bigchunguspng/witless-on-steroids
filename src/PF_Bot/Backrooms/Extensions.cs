using System.Globalization;

namespace PF_Bot.Backrooms;

public static partial class Extensions
{
    //

    public static string HOURS_ED(int hours) => ED(hours, "", "а", "ов");
    public static string  MINS_ED(int mins ) => ED(mins,  "у", "ы", "");

    private static string ED(int x, string one, string twoFour, string any)
    {
        if (x % 10 > 4 || x % 10 == 0 || x is > 10 and < 15) return any;
        else if (x % 10 > 1) return twoFour;
        else return one;
    }

    //

    private static readonly Regex Errors = new(@"One or more errors occurred. \((\S*(\s*\S)*)\)");

    public static string GetFixedMessage(this Exception e)
    {
        var message = e.Message;
        return Errors.ExtractGroup(1, message, s => s, message)!;
    }

    // UI

    public static string ReadableFileSize(this long bytes)
    {
        var kbs = bytes / 1024F;
        var mbs = kbs   / 1024F;
        return mbs >= 100 ? UI($"{mbs:F1} МБ")
            :  mbs >=   1 ? UI($"{mbs:F2} МБ")
            :  kbs >=   1 ? UI($"{kbs:F0} КБ")
            :                  $"{bytes} байт";
    }

    private static readonly CultureInfo _culture_UI = CultureInfo.GetCultureInfo("fi"/*nna jerk it))*/);

    public static string UI
        (this FormattableString formattable) => formattable.ToString(_culture_UI);
}