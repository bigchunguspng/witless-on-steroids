using PF_Bot.Features_Main.Memes.Core.Generators;
using PF_Bot.Features_Main.Memes.Core.Options;
using PF_Bot.Features_Main.Memes.Core.Shared;

namespace PF_Bot.Features_Main.Memes.Commands;

public class Snap : Meme_Core<string>
{
    private static readonly FontWizard _fontWizard = new ("rg");
    private static readonly ColorWizard _colorWizardBack = new ("_");
    private static readonly ColorWizard _colorWizardText = new ("#");

    private MemeOptions_Snap _options;

    protected override IMemeGenerator<string> MemeMaker => new SnapChat(_options);

    protected override string VideoName => "piece_fap_bot-snap.mp4";

    protected override string Log_STR => "SNAP";
    protected override string Log_CMD => "/snap";
    protected override string Suffix  => "Snap";

    protected override string? DefaultOptions => Data.Options?.Snap;


    protected override Task Run() => RunInternal("snap");

    protected override bool ResultsAreRandom => _options.RandomTextOffset || _options.FontOption.IsRandom;

    protected override void ParseOptions()
    {
        _options.RandomTextOffset = MemeOptions.CheckAndCut(_r_randomOffset);

        _options.CustomColorBack = _colorWizardBack.CheckAndCut(MemeOptions);
        _options.CustomColorText = _colorWizardText.CheckAndCut(MemeOptions);

        _options.FontOption = _fontWizard.CheckAndCut(MemeOptions);

        _options.MinFontSizeMultiplier = MemeOptions.GetInt(_r_fontSize, 10, group: 2);
        _options.   FontSizeMultiplier = MemeOptions.GetInt(_r_fontSizeMin, 100);
        _options.CardOpacity           = MemeOptions.GetInt(_r_opacity, 62).ClampByte().Clamp100();
        _options.TextOffset            = MemeOptions.GetInt(_r_offset,  50);

        _options.WrapText = MemeOptions.CheckAndCut(_r_nowrap).Failed();
    }

    protected override string GetMemeText(string? text)
    {
        var generate = text.IsNull_OrEmpty();
        var capitalize = MemeOptions.CheckCaps(_r_caps, generate);

        var caption = generate ? Baka.Generate() : text!;

        return capitalize ? caption.InLetterCase(LetterCase.Upper) : caption;
    }

    private const string
        _r_randomOffset = "!!";

    private static readonly Regex
        _r_opacity     = new(     @"(\d{1,3})(%)",  RegexOptions.Compiled),
        _r_offset      = new(     @"(\d{1,3})(!)",  RegexOptions.Compiled),
        _r_fontSizeMin = new(     @"(\d{1,3})("")", RegexOptions.Compiled),
        _r_fontSize    = new(@"(min)(\d{1,3})("")", RegexOptions.Compiled);
}