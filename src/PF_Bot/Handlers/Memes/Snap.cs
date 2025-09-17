using PF_Bot.Core.Memes.Generators;
using PF_Bot.Core.Memes.Options;
using PF_Bot.Core.Memes.Shared;

namespace PF_Bot.Handlers.Memes;

public class Snap : MakeMemeCore<string>
{
    private static readonly FontWizard _fontWizard = new ("rg");
    private static readonly ColorWizard _colorWizardBack = new ("_");
    private static readonly ColorWizard _colorWizardText = new ("#");

    private MemeOptions_Snap _options;

    protected override IMemeGenerator<string> MemeMaker => new SnapChat(_options);

    protected override Regex _rgx_cmd { get; } = new(@"^\/snap(\S*)", RegexOptions.Compiled);

    protected override string VideoName => "piece_fap_bot-snap.mp4";

    protected override string Log_STR => "SNAP";
    protected override string Log_CMD => "/snap";
    protected override string Suffix  => "Snap";

    protected override string? DefaultOptions => Data.Options?.Snap;


    protected override Task Run() => RunInternal("snap");

    protected override bool ResultsAreRandom => _options.RandomOffset || _options.FontOption.IsRandom;

    protected override void ParseOptions()
    {
        _options.RandomOffset = Options.CheckAndCut(_r_random);

        _options.CustomColorBack = _colorWizardBack.CheckAndCut(Options);
        _options.CustomColorText = _colorWizardText.CheckAndCut(Options);

        _options.FontOption = _fontWizard.CheckAndCut(Options);

        _options.MinSizeMultiplier  = Options.GetInt(_r_fontMS,  10, group: 2);
        _options.FontSizeMultiplier = Options.GetInt(_r_fontSM, 100);
        _options.CardOpacity        = Options.GetInt(_r_opacity, 62);
        _options.CardOffset         = Options.GetInt(_r_offset,  50);

        _options.WrapText = Options.CheckAndCut(_r_nowrap).Failed();
    }

    protected override string GetMemeText(string? text)
    {
        var generate = text.IsNull_OrEmpty();
        var capitalize = Options.CheckCaps(_r_caps, generate);

        var caption = generate ? Baka.Generate() : text!;

        return capitalize ? caption.InLetterCase(LetterCase.Upper) : caption;
    }

    private const string
        _r_random  = "!!";

    private static readonly Regex
        _r_opacity = new(     @"(\d{1,3})(%)",  RegexOptions.Compiled),
        _r_offset  = new(     @"(\d{1,3})(!)",  RegexOptions.Compiled),
        _r_fontSM  = new(     @"(\d{1,3})("")", RegexOptions.Compiled),
        _r_fontMS  = new(@"(min)(\d{1,3})("")", RegexOptions.Compiled);
}