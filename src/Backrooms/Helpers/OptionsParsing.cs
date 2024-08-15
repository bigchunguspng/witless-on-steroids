using Witlesss.Commands.Meme.Core;

namespace Witlesss.Backrooms.Helpers;

public static class OptionsParsing
{
    public static bool Check(MemeRequest request, Regex regex)
    {
        return !request.Empty && regex.IsMatch(request.Dummy);
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
        if (match.Success == false) return null;

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
        if (value is null) return @default;

        return int.Parse(value);
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

    /// <summary>
    /// Use UPPERCASE if text is generated or option provided via commnand (ignore default options).
    /// </summary>
    public static bool CheckCaps(MemeRequest request, Regex caps, bool generate)
    {
        return Check(request, caps) && (generate || caps.IsMatch(request.Command));
    }
}