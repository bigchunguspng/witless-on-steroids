using PF_Bot.Routing.Commands;

namespace PF_Bot.Backrooms.Helpers;

// todo make it extension class
public static class ArgumentParsing
{
    private static readonly char[] _separators = [' ', '\n'];

    /// Splits arguments by whitespaces and line breaks.
    public static string[] SplitN(this string? arguments, int count = int.MaxValue)
    {
        return arguments is null ? [] : arguments.Split(_separators, count, StringSplitOptions.RemoveEmptyEntries);
    }

    public static bool HasArguments(this string text)
    {
        return text.Contains(' ') || text.Contains('\n');
    }

    public static bool HasLongArgument(this string text, out long value)
    {
        value = 0;
        var words = text.Split();
        return words.Length > 1 && long.TryParse(words[1], out value);
    }

    public static bool HasIntArgument(this string text, out int value)
    {
        value = 0;
        var words = text.Split();
        return words.Length > 1 && int.TryParse(words[1], out value);
    }

    public static bool HasIntArgument(this CommandContext c, out int value)
    {
        value = 0;
        if (c.Args is null) return false;

        var arg = c.Args.SplitN(2)[0];
        return int.TryParse(arg, out value);
    }

    public static bool HasDoubleArgument(this CommandContext c, out double value)
    {
        value = 0;
        return c.Args != null && c.Args.SplitN(2)[0].TryParseF64_Invariant(out value);
    }

    /// Tryies to parse string as <see cref="double"/>.
    /// Both ',' and '.' are valid fraction separators.
    public static bool TryParseF64_Invariant
        (this string text, out double value)
        => double.TryParse(text.Replace(',', '.'), out value);

    public static (bool Failed, TimeSpan Start, TimeSpan Length) GetCutTimecodes(string[]? s)
    {
        var zero = TimeSpan.Zero;

        if (s is null) return (true, zero, zero);

        var len = s.Length;
        if     (len == 1 && s[0].IsTimeSpan(out var length)) return (false, zero,  length);      // [++]----]
        if     (len >= 2 && s[0].IsTimeSpan(out var  start))
        {
            if (len == 3 && s[2].IsTimeSpan(out var    end)) return (false, start, end - start); // [-[++]--]
            if             (s[1].IsTimeSpan(out     length)) return (false, start, length);      // [-[++]--]
            else                                             return (false, start, zero);        // [-[+++++]
        }
        else                                                 return (true,  zero,  zero);        // [-------]
    }

    private static readonly Regex
        _rgx_seconds = new(@"^(?:(\d+)[:;^Жж])?(\d+(?:[.,юб]\d+)?)$", RegexOptions.Compiled),
        _rgx_comma   = new("[,юб]", RegexOptions.Compiled);

    private static bool IsTimeSpan(this string text, out TimeSpan span)
    {
        span = TimeSpan.Zero;
        text = text.TrimStart('-');

        var match = _rgx_seconds.Match(text);
        if (match.Failed()) return false;

        var s = _rgx_comma.Replace(match.Groups[2].Value, ".");
        var m = match.GroupOrNull(1) ?? "0";

        if (double.TryParse(s, out var seconds)) span  = TimeSpan.FromSeconds(seconds);
        if (double.TryParse(m, out var minutes)) span += TimeSpan.FromMinutes(minutes);

        return true;
    }
}