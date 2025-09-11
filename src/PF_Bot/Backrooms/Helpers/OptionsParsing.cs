using PF_Bot.Features.Generate.Memes.Core;

namespace PF_Bot.Backrooms.Helpers;

public static class OptionsParsing
{
    public static bool Check(MemeRequest request, Regex regex)
    {
        return request.Empty.Janai() && regex.IsMatch(request.Dummy);
    }

    public static bool CheckAndCut(MemeRequest request, Regex regex)
    {
        if (request.Empty) return false;

        var match = regex.Match(request.Dummy);
        if (match.Success)
        {
            CutCaptureOut(match.Groups[1], request);
        }

        return match.Success;
    }

    public static string? GetValue(MemeRequest request, Regex regex, int group = 1)
    {
        if (request.Empty) return null;

        var match = regex.Match(request.Dummy);
        if (match.Failed()) return null;

        var value = match.Groups[group].Value;
        for (var i = match.Groups.Count - 1; i > 0; i--)
        {
            CutCaptureOut(match.Groups[i], request);
        }

        return value;
    }

    public static int GetInt(MemeRequest request, Regex regex, int @default, int group = 1)
    {
        var value = GetValue(request, regex, group);
        if (value.IsNull_OrEmpty()) return @default;

        return int.Parse(value);
    }

    public static float GetFraction(MemeRequest request, Regex regex, int @default, int group = 1)
    {
        if (Check(request, regex).Failed()) return 0;

        var value = GetValue(request, regex, group);
        var (number, text) = value.IsNull_OrEmpty() ? (@default, @default.ToString()) : (int.Parse(value), value);
        return number / MathF.Pow(10, text.Length);
    }

    public static void CutCaptureOut(Capture group, MemeRequest request)
    {
        if (group.Length == 0) return;

        var newLength = request.Dummy.Length - group.Length + 1;

        var source = request.Dummy.AsSpan();
        Span<char> destination = stackalloc char[newLength];

        source
            .Slice(0, group.Index)
            .CopyTo(destination);
        destination[group.Index] = '_';
        source
            .Slice(group.Index + group.Length)
            .CopyTo(destination.Slice(group.Index + 1));

        request.Dummy = new string(destination);
    }

    /// Use UPPERCASE if text is generated or option provided via commnand (ignore default options).
    public static bool CheckCaps(MemeRequest request, Regex caps, bool generate)
    {
        return Check(request, caps) && (generate || caps.IsMatch(request.Command));
    }
}