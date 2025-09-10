using System.Diagnostics.CodeAnalysis;
using PF_Bot.Backrooms.Helpers;
using PF_Bot.Core.Meme.Options;
using PF_Bot.Features.Generate.Memes.Core;

namespace PF_Bot.Core.Meme.Fonts;

/// Use this to parse font option.
/// Created once for a meme maker.
public class FontWizard
{
    private readonly Regex _regex;
    private readonly string _fontKeyDefault;

    public FontWizard
    (
        string fontKeyDefault,
        [StringSyntax("Regex")] string cmdRegex,
        [StringSyntax("Regex")] string? x = null
    )
    {
        var codes = string.Join('|', FontStorage.Families.Keys);
        _fontKeyDefault = fontKeyDefault;
        _regex = new Regex($@"^\/{cmdRegex}\S*(?:({codes}|\^\^)(-[bi]{{1,2}})?){x}\S*", RegexOptions.IgnoreCase);
    }

    public FontOption CheckAndCut(MemeRequest request)
    {
        var fontKeyIsDefault = true;
        var fontKey = _fontKeyDefault;
        var styleKey = (string?)null;

        var match = _regex.Match(request.Dummy);

        var success = !request.Empty && match.Success;
        if (success)
        {
            fontKeyIsDefault = false;

            var g1 = match.Groups[1];
            fontKey = g1.Value;
            var g2 = match.Groups[2];
            styleKey = g2.Success ? g2.Value : null;

            for (var i = match.Groups.Count - 1; i > 0; i--)
            {
                OptionsParsing.CutCaptureOut(match.Groups[i], request);
            }
        }

        var random = fontKey is "^^";

        return new FontOption(fontKey, styleKey, random, fontKeyIsDefault);
    }
}