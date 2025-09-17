namespace PF_Bot.Core.Memes.Options;

public static class MemeOptions_Extensions
{
    public static bool Check
        (this MemeOptionsContext context, Regex regex)
        => context.Empty.Janai()
        && regex.IsMatch(context.Buffer);

    public static bool Check
        (this MemeOptionsContext context, string option)
        => context.Empty.Janai()
        && context.Buffer.Contains(option, StringComparison.Ordinal);

    public static bool CheckAndCut
        (this MemeOptionsContext context, Regex regex, int group = 1)
    {
        if (context.Empty) return false;

        var match = regex.Match(context.Buffer);
        if (match.Success)
        {
            context.CutCaptureOut(match.Groups[group]);
        }

        return match.Success;
    }

    public static bool CheckAndCut
        (this MemeOptionsContext context, string option)
    {
        if (context.Empty) return false;

        var index = context.Buffer.IndexOf(option, StringComparison.Ordinal);

        var success = index >= 0;
        if (success)
        {
            context.CutCaptureOut(index, option.Length);
        }

        return success;
    }

    public static string? GetValue
        (this MemeOptionsContext context, Regex regex, int group = 1)
    {
        if (context.Empty) return null;

        var match = regex.Match(context.Buffer);
        if (match.Failed()) return null;

        var value = match.Groups[group].Value;
        for (var i = match.Groups.Count - 1; i > 0; i--)
        {
            context.CutCaptureOut(match.Groups[i]);
        }

        return value;
    }

    public static int GetInt
        (this MemeOptionsContext context, Regex regex, int @default, int group = 1)
    {
        var value = GetValue(context, regex, group);
        if (value.IsNull_OrEmpty()) return @default;

        return int.Parse(value);
    }

    /// 75 -> 0.75; 5 -> 0.5; 0.5 -> 0.05.
    public static float GetFraction
        (this MemeOptionsContext context, Regex regex, int @default, int group = 1)
    {
        var value = GetValue(context, regex, group);
        if (value == null) return 0;

        var (number, text) = value.IsNull_OrEmpty()
            ? (@default, @default.ToString())
            : (int.Parse(value), value);
        return number / MathF.Pow(10, text.Length);
    }

    public static void CutCaptureOut
        (this MemeOptionsContext context, Capture group) =>
        context.CutCaptureOut(group.Index, group.Length);

    public static void CutCaptureOut
        (this MemeOptionsContext context, int index, int length)
    {
        var b = context.Buffer;
        if (length == 0) return;

        var newLength = context.Buffer.Length - length + 1;

        var source = context.Buffer.AsSpan();
        Span<char> destination = stackalloc char[newLength];

        source
            .Slice(0, index)
            .CopyTo(destination);
        destination[index] = '▒';
        source
            .Slice(index + length)
            .CopyTo(destination.Slice(index + 1));

        context.Buffer = new string(destination);
#if DEBUG
        Log($"[CutCaptureOut: {index}+{length}]: {b} -> {context.Buffer}", LogLevel.Debug);
#endif
    }

    /// Use UPPERCASE if text is generated or option provided via commnand (ignore default options).
    public static bool CheckCaps
        (this MemeOptionsContext context, string option, bool generate)
        => Check(context, option)
        && (generate || (context.CommandOptions?.Contains(option) ?? false));
}