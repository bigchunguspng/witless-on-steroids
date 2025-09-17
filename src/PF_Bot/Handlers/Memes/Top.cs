using PF_Bot.Core.Memes.Generators;
using PF_Bot.Core.Memes.Options;
using PF_Bot.Core.Memes.Shared;

namespace PF_Bot.Handlers.Memes
{
    public class Top : MakeMemeCore<string>
    {
        private static readonly FontWizard _fontWizard = new ("ft");
        private static readonly ColorWizard _colorWizard = new ("#");

        private MemeOptions_Top _options;

        protected override IMemeGenerator<string> MemeMaker => new IFunnyBrazil(_options);

        protected override Regex _rgx_cmd { get; } = new(@"^\/top(\S*)", RegexOptions.Compiled);

        protected override string VideoName => "piece_fap_bot-top.mp4";

        protected override string Log_STR => "TOP";
        protected override string Log_CMD => "/top";
        protected override string Suffix  => "Top";

        protected override string? DefaultOptions => Data.Options?.Top;


        protected override Task Run() => RunInternal("top");

        protected override bool ResultsAreRandom => _options.FontOption.IsRandom;

        protected override void ParseOptions()
        {
            _options.CustomColor = _colorWizard.CheckAndCut(Options);
            _options.FontOption = _fontWizard.CheckAndCut(Options);

            _options.CropPercent        = Options.GetInt(_r_crop,     0);
            _options.MinSizeMultiplier  = Options.GetInt(_r_fontMS,  10, group: 2);
            _options.FontSizeMultiplier = Options.GetInt(_r_fontSM, 100);

            _options.WrapText         = Options.CheckAndCut(_r_nowrap ).Failed();
            _options.BackInBlack      = Options.CheckAndCut(_r_blackBG);
            _options.ForceCenter      = Options.CheckAndCut(_r_colorPC);
            _options.PickColor        = Options.CheckAndCut(_r_colorPP);
            _options.UseLeftAlignment = Options.CheckAndCut(_r_left   );
            _options.UltraThinCard    = Options.CheckAndCut(_r_thinner);
            _options.ThinCard         = Options.CheckAndCut(_r_thin   );
        }

        protected override string GetMemeText(string? text)
        {
            var generate = text.IsNull_OrEmpty();
            var capitalize = Options.CheckCaps(_r_caps, generate);

            var caption = generate ? Baka.Generate() : text!;

            if (_options.FontOption.IsDefault)
                _options.FontOption.FontKey = caption.IsMostlyCyrillic() ? "sg" : "ft";

            return capitalize ? caption.InLetterCase(LetterCase.Upper) : caption;
        }

        private const string
            _r_left    = "la",
            _r_thin    = "mm",
            _r_colorPP = "pp",
            _r_blackBG = "ob";

        private static readonly Regex
            _r_thinner = new("mm(!)", RegexOptions.Compiled),
            _r_colorPC = new("pp(!)", RegexOptions.Compiled),
            _r_crop    = new(   @"(-?\d{1,2})(%)",  RegexOptions.Compiled),
            _r_fontSM  = new(     @"(\d{1,3})("")", RegexOptions.Compiled),
            _r_fontMS  = new(@"(min)(\d{1,3})("")", RegexOptions.Compiled);
    }
}