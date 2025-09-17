using PF_Bot.Core.Memes.Generators;
using PF_Bot.Core.Memes.Options;
using PF_Bot.Core.Memes.Shared;
using PF_Bot.Handlers.Memes.Core;
using static PF_Bot.Backrooms.Helpers.OptionsParsing;

namespace PF_Bot.Handlers.Memes;

public class Snap : MakeMemeCore<string>
{
    private static readonly FontWizard _fontWizard = new ("rg", "snap");
    private static readonly ColorWizard _colorWizardBack = new ("_");
    private static readonly ColorWizard _colorWizardText = new ("#");

    private MemeOptions_Snap _options;

    protected override IMemeGenerator<string> MemeMaker => new SnapChat(_options);

    protected override Regex _rgx_cmd { get; } = new(@"^\/snap(\S*)", RegexOptions.Compiled);

    protected override string VideoName => "piece_fap_bot-snap.mp4";

    protected override string Log_STR => "SNAP";
    protected override string Command => "/snap";
    protected override string Suffix  => "Snap";

    protected override string? DefaultOptions => Data.Options?.Snap;


    protected override Task Run() => RunInternal("snap");

    protected override bool ResultsAreRandom => _options.RandomOffset || _options.FontOption.IsRandom;

    protected override void ParseOptions()
    {
        _options.RandomOffset = CheckAndCut(Request, _r_random);

        _options.CustomColorBack = _colorWizardBack.CheckAndCut(Request);
        _options.CustomColorText = _colorWizardText.CheckAndCut(Request);

        _options.FontOption = _fontWizard.CheckAndCut(Request);

        _options.MinSizeMultiplier  = GetInt(Request, _r_fontMS,  10, group: 2);
        _options.FontSizeMultiplier = GetInt(Request, _r_fontSM, 100);
        _options.CardOpacity        = GetInt(Request, _r_opacity, 62);
        _options.CardOffset         = GetInt(Request, _r_offset,  50);

        _options.WrapText = CheckAndCut(Request, _r_nowrap).Failed();
    }

    protected override string GetMemeText(string? text)
    {
        var generate = text.IsNull_OrEmpty();
        var capitalize = CheckCaps(Request, _r_caps, generate);

        var caption = generate ? Baka.Generate() : text!;

        return capitalize ? caption.InLetterCase(LetterCase.Upper) : caption;
    }

    private static readonly Regex
        _r_random  = new(@"^\/snap\S*(!!)\S*",           RegexOptions.Compiled),
        _r_opacity = new(@"^\/snap\S*?(\d{1,3})(%)\S*",  RegexOptions.Compiled),
        _r_offset  = new(@"^\/snap\S*?(\d{1,3})(!)\S*",  RegexOptions.Compiled),
        _r_fontSM  = new(@"^\/snap\S*?(\d{1,3})("")\S*", RegexOptions.Compiled),
        _r_fontMS  = new(@"^\/snap\S*?(min)(\d{1,3})("")\S*", RegexOptions.Compiled);
}