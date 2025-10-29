using PF_Bot.Features_Main.Memes.Core.Generators;
using PF_Bot.Features_Main.Memes.Core.Options;
using PF_Bot.Features_Main.Memes.Core.Shared;

namespace PF_Bot.Features_Main.Memes.Commands;

public class Meme : Meme_Core<TextPair>
{
    private static readonly FontWizard _fontWizard = new ("im");
    private static readonly ColorWizard _colorWizardBack = new ("_");
    private static readonly ColorWizard _colorWizardText = new ("#");
    private static readonly ColorWizard _colorWizardShad = new ("!");

    private MemeOptions_Meme _options;

    protected override IMemeGenerator<TextPair> MemeMaker => new MemeGenerator(_options);

    protected override string VideoName => "piece_fap_bot-meme.mp4";

    protected override string Log_STR => "MEME";
    protected override string Log_CMD => "/meme";
    protected override string Suffix  => "Meme";

    protected override string? DefaultOptions => Data.Options?.Meme;


    protected override Task Run() => RunInternal("meme");

    protected override bool ResultsAreRandom
        => _options.RandomTextColor
        || _options.FontOption.IsRandom
        || MemeOptions.Check(_r_add_bottom) && Args!.Contains('\n').Janai(); // (random bottom text)

    protected override void ParseOptions()
    {
        _options.CustomColorBack = _colorWizardBack.CheckAndCut(MemeOptions);
        _options.CustomColorText = _colorWizardText.CheckAndCut(MemeOptions);
        _options.CustomColorShad = _colorWizardShad.CheckAndCut(MemeOptions);

        _options.FontOption = _fontWizard.CheckAndCut(MemeOptions);

        _options.FontSizeMultiplier = MemeOptions.GetInt(_r_fontSize, 100);
        _options.ShadowOpacity      = MemeOptions.GetInt(_r_shadowO, 100).ClampByte().Clamp100();
        _options.ShadowThickness    = MemeOptions.GetInt(_r_shadowT, 100);
        _options.TextOffset         = MemeOptions.GetInt(_r_offset, -1);

        _options.RandomTextOffset   = MemeOptions.CheckAndCut(_r_randomOffset);
        _options.WrapText           = MemeOptions.CheckAndCut(_r_nowrap).Failed();
        _options.RandomTextColor    = MemeOptions.CheckAndCut(_r_colorText);
        _options.AbsolutelyNoMargin = MemeOptions.CheckAndCut(_r_noMarginDude);
        _options.NoMargin           = MemeOptions.CheckAndCut(_r_noMargin);
    }

    protected override TextPair GetMemeText(string? text)
    {
        var generate = text.IsNull_OrEmpty();
        var capitalize = MemeOptions.CheckCaps(_r_caps, generate);

        var lowerCase      = MemeOptions.Check(_r_lowerCase);
        var addBottomText  = MemeOptions.Check(_r_add_bottom);
        var onlyBottomText = MemeOptions.Check(_r_only_bottom);
        var onlyTopText    = MemeOptions.Check(_r_top_only);

        string a, b;

        if (_options.FloatingCaptionMode)
        {
            a = generate ? Baka.Generate() : text!;
            b = "";
        }
        else if (generate)
        {
            var (genA, genB) = (true, true);

            var chance = Random.Shared.Next(6);

            if /**/ (onlyBottomText) genA = false;
            else if (onlyTopText)    genB = false;
            else if (chance == 0)    genA = false;
            else if (chance == 1)    genB = false;

            a = genA ? Baka.Generate() : "";
            b = genB ? Baka.Generate() : "";

            if (genA && onlyTopText.Janai() && (genB ? a.Length > b.Length : a.Length > 64))
            {
                // bigger text (or big enough one) should be at the bottom
                (a, b) = (b, a);
            }
        }
        else
        {
            Debug.Assert(text != null);

            var separator = TryGetSeparator(text);
            if (separator is not null)
            {
                var s = text.Split(separator, 2);
                (a, b) = (s[0], s[1]);
            }
            else
            {
                a = text;
                b = addBottomText ? Baka.Generate() : "";
            }
        }

        return new TextPair(AdjustCase(a), AdjustCase(b));

        string AdjustCase
            (string s) => lowerCase ? s.ToLower() : generate || capitalize ? s.ToUpper() : s;
    }

    private static readonly string[] separators = ["\n\n\n\n", "\n\n\n", "\n\n", "\n"];

    private static string? TryGetSeparator(string text)
    {
        return separators.FirstOrDefault(text.Contains);
    }

    private const string
        _r_add_bottom   = "s",
        _r_only_bottom  = "d",
        _r_top_only     = "t",
        _r_lowerCase    = "lo",
        _r_colorText    = "cc",
        _r_randomOffset = "!!",
        _r_noMargin     = "mm",
        _r_noMarginDude = "mm!";

    private static readonly Regex
        _r_fontSize = new(@"(\d{1,3})("")", RegexOptions.Compiled),
        _r_shadowO  = new(@"(\d{1,3})(%)",  RegexOptions.Compiled),
        _r_shadowT  = new(@"(\d{1,3})(w)",  RegexOptions.Compiled),
        _r_offset   = new(@"(\d{1,3})(!)",  RegexOptions.Compiled);
}