using PF_Bot.Features_Main.Memes.Core.Generators;
using PF_Bot.Features_Main.Memes.Core.Options;
using PF_Bot.Features_Main.Memes.Core.Shared;

namespace PF_Bot.Features_Main.Memes.Commands;

public class Top : Meme_Core<string>
{
    private static readonly FontWizard _fontWizard = new ("ft");
    private static readonly ColorWizard _colorWizard = new ("#");

    private MemeOptions_Top _options;

    protected override IMemeGenerator<string> MemeMaker => new IFunnyBrazil(_options);

    protected override string? DefaultOptions => Data.Options?.Top;

    protected override MemeMakerContext Ctx => MemeMakerContext.Top;


    protected override Task Run() => RunInternal("top");

    protected override bool ResultsAreRandom => _options.FontOption.IsRandom;

    protected override void ParseOptions()
    {
        _options.CustomColor = _colorWizard.CheckAndCut(MemeOptions);
        _options.FontOption  =  _fontWizard.CheckAndCut(MemeOptions);

        _options.CropPercent           = MemeOptions.GetInt(_r_crop, 0).Clamp(-100, 100).ClampSbyte();
        _options.MinFontSizeMultiplier = MemeOptions.GetInt(_r_fontSizeMin, 10, group: 2);
        _options.   FontSizeMultiplier = MemeOptions.GetInt(_r_fontSize, 100);

        _options.WrapText             = MemeOptions.CheckAndCut(_r_nowrap).Failed();
        _options.BackInBlack          = MemeOptions.CheckAndCut(_r_blackBg);
        _options.PickColor_FromCenter = MemeOptions.CheckAndCut(_r_colorPPX);
        _options.PickColor            = MemeOptions.CheckAndCut(_r_colorPP);
        _options.TextLeftAlignment    = MemeOptions.CheckAndCut(_r_left);
        _options.UltraThinCard        = MemeOptions.CheckAndCut(_r_thinner);
        _options.     ThinCard        = MemeOptions.CheckAndCut(_r_thin);
    }

    protected override string GetMemeText(string? text)
    {
        var generate = text.IsNull_OrEmpty();
        var capitalize = MemeOptions.CheckCaps(_r_caps, generate);

        var caption = generate ? Baka.Generate() : text!;

        if (_options.FontOption.IsDefault)
            _options.FontOption.FontKey = caption.IsMostlyCyrillic() ? "sg" : "ft";

        return capitalize ? caption.InLetterCase(LetterCase.Upper) : caption;
    }

    private const string
        _r_left    = "la",
        _r_thin    = "mm",
        _r_colorPP = "pp",
        _r_blackBg = "ob";

    private static readonly Regex
        _r_thinner  = new("mm(!)", RegexOptions.Compiled),
        _r_colorPPX = new("pp(!)", RegexOptions.Compiled),
        _r_crop        = new(   @"(-?\d{1,2})(%)",  RegexOptions.Compiled),
        _r_fontSize    = new(     @"(\d{1,3})("")", RegexOptions.Compiled),
        _r_fontSizeMin = new(@"(min)(\d{1,3})("")", RegexOptions.Compiled);
}