using System.Text.RegularExpressions;

namespace Witlesss.Backrooms;

public static class OptionsParsing
{
    public static bool CheckMatch(ref string dummy, Regex regex)
    {
        var match = regex.Match(dummy);
        if (match.Success) CutCaptureOut(match.Groups[1], ref dummy);

        return match.Success;
    }

    public static void CutCaptureOut(Capture group, ref string text)
    {
        text = text.Remove(group.Index) + "_" + text.Substring(group.Index + group.Length);
    }
}