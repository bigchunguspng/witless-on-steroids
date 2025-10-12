namespace PF_Bot.Backrooms;

public static class Extensions_Parsing
{
    // SPLIT N

    private static readonly char[] _separators = [' ', '\n'];

    /// Splits arguments by whitespaces and line breaks.
    public static string[] SplitN
        (this string? text, int count = int.MaxValue)
        => text is null
            ? []
            : text.Split(_separators, count, StringSplitOptions.RemoveEmptyEntries);

    /// <seealso cref="SplitN"/>
    public static bool CanBeSplitN
        (this string text)
        => text.IndexOfAny(_separators) >= 0;

    // TRY PARSE NUMBERS

    public static bool TryParseAsLong
        (this string? text, out long value)
    {
        value = 0;
        return text != null && long.TryParse(text, out value);
    }

    public static bool TryParseAsInt
        (this string? text, out int value)
    {
        value = 0;
        return text != null && int.TryParse(text, out value);
    }

    public static bool TryParseAsDouble
        (this string? text, out double value)
    {
        value = 0;
        return text != null && text.TryParseF64_Invariant(out value);
    }

    /// Tryies to parse string as <see cref="double"/>.
    /// Both ',' and '.' are valid fraction separators.
    public static bool TryParseF64_Invariant
        (this string text, out double value)
        => double.TryParse(text.Replace(',', '.'), out value);

    // TIME SPANS

    /// Expected syntax variants:
    /// <br/> <c>length</c>
    /// <br/> <c>start - end</c>
    /// <br/> <c>start length</c>
    /// <br/> <c>start fgsfds</c> (till the end)
    /// <seealso cref="TryParseTime"/>
    public static bool GetCutTimecodes
        (this string[]? args, out TimeSpan start, out TimeSpan length)
    {
        start = length = TimeSpan.Zero;

        if (args == null) return false;

        var     len = args.Length;
        if     (len == 1 && args[0].TryParseTime(out  length)) return true;          // ==______ <- 2
        if     (len >= 2 && args[0].TryParseTime(out   start))                       // __._____ <- 2 …
        {
            if (len >= 3 && args[2].TryParseTime(out var end)) length = end - start; // __===___ <- 2 - 5
            else            args[1].TryParseTime(out  length);  // __====== <- 2 .  OR  __===___ <- 2 3
            return true;
        }
        else return false;
    }

    private static readonly Regex
        _rgx_seconds = new(@"^(?:(\d+)[:;^Жж])?(\d+(?:[.,юб]\d+)?)$", RegexOptions.Compiled),
        _rgx_comma   = new("[,юб]", RegexOptions.Compiled);

    /// Expected syntax: <c>[minutes][:;^Жж][seconds][.,юб][ms]</c>.
    private static bool TryParseTime
        (this string text, out TimeSpan span)
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