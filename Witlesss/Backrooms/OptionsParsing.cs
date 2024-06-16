using System;
using System.Text.RegularExpressions;
using Witlesss.Commands.Meme;

namespace Witlesss.Backrooms;

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

    public static int GetInt(MemeRequest request, Regex regex, int @default)
    {
        if (request.Empty) return @default;

        var match = regex.Match(request.Dummy);
        if (match.Success == false) return @default;

        var value = int.Parse(match.Groups[1].Value);
        for (var i = match.Groups.Count - 1; i > 0; i--)
        {
            CutCaptureOut(match.Groups[i], request);
        }

        return value;
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

    /*public static void ParseColorOption(Regex regex, ref string dummy, ref Color colorProperty, ref bool useColorProperty)
    {
        var c = regex.Match(dummy).Groups[1].Value;
        dummy = dummy.Replace(c, "");
        if (c == c.ToLower() || c == c.ToUpper()) c = c.ToLetterCase(LetterCaseMode.Sentence);
        var b = Enum.IsDefined(typeof(KnownColor), c);
        if (b) colorProperty = Color.FromName(c);
        else useColorProperty = false;
    }*/
}