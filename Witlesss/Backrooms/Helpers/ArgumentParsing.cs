using System;

namespace Witlesss.Backrooms.Helpers;

public static class ArgumentParsing
{
    private static readonly char[] _separators = [' ', '\n'];

    /// <summary>
    /// Splits arguments by whitespaces and line breaks.
    /// </summary>
    public static string[] SplitN(this string? arguments, int count = int.MaxValue)
    {
        return arguments is null ? [] : arguments.Split(_separators, count, StringSplitOptions.RemoveEmptyEntries);
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
        if (c.Args is null) return false;

        var arg = c.Args.SplitN()[0];
        return double.TryParse(arg.Replace('.', ','), out value);
    }
}