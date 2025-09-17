using System.Diagnostics.CodeAnalysis;

namespace PF_Bot.Core.Memes.Options;

/// Use this to parse font option.
/// Created once for a meme maker.
public class FontWizard
{
    private readonly Regex _regex;
    private readonly string _fontKeyDefault;

    public FontWizard
    (
        string fontKeyDefault,
        [StringSyntax("Regex")] string? marker = null
    )
    {
        var codes = string.Join('|', FontStorage.Families.Keys);
        _fontKeyDefault = fontKeyDefault;
        _regex = new Regex($@"(?:({codes}|\^\^)(-[bi]{{1,2}})?){marker}");
    }

    public FontOption CheckAndCut(MemeOptionsContext context)
    {
        var fontKeyIsDefault = true;
        var fontKey = _fontKeyDefault;
        var styleKey = (string?)null;

        var match = _regex.Match(context.Buffer);

        var success = context.Empty.Janai() && match.Success;
        if (success)
        {
            fontKeyIsDefault = false;

            var g1 = match.Groups[1];
            fontKey = g1.Value;
            var g2 = match.Groups[2];
            styleKey = g2.Success ? g2.Value : null;

            for (var i = match.Groups.Count - 1; i > 0; i--)
            {
                context.CutCaptureOut(match.Groups[i]);
            }
        }

        var random = fontKey is "^^";

        return new FontOption(fontKey, styleKey, random, fontKeyIsDefault);
    }
}