using PF_Bot.Core.Memes.Generators;
using PF_Bot.Core.Memes.Options;
using PF_Bot.Core.Memes.Shared;
using PF_Bot.Handlers.Memes.Core;
using static PF_Bot.Backrooms.Helpers.OptionsParsing;

namespace PF_Bot.Handlers.Memes
{
    public class Top : MakeMemeCore<string>
    {
        private static readonly FontWizard _fontWizard = new ("ft", "top");
        private static readonly ColorWizard _colorWizard = new ("#");

        private MemeOptions_Top _options;

        protected override IMemeGenerator<string> MemeMaker => new IFunnyBrazil(_options);

        protected override Regex _rgx_cmd { get; } = new(@"^\/top(\S*)", RegexOptions.Compiled);

        protected override string VideoName => "piece_fap_bot-top.mp4";

        protected override string Log_STR => "TOP";
        protected override string Command => "/top";
        protected override string Suffix  => "Top";

        protected override string? DefaultOptions => Data.Options?.Top;


        protected override Task Run() => RunInternal("top");

        protected override bool ResultsAreRandom => _options.FontOption.IsRandom;

        protected override void ParseOptions()
        {
            _options.CustomColor = _colorWizard.CheckAndCut(Request);
            _options.FontOption = _fontWizard.CheckAndCut(Request);

            _options.CropPercent        = GetInt(Request, _r_crop,     0);
            _options.MinSizeMultiplier  = GetInt(Request, _r_fontMS,  10, group: 2);
            _options.FontSizeMultiplier = GetInt(Request, _r_fontSM, 100);

            _options.WrapText         = CheckAndCut(Request, _r_nowrap ).Failed();
            _options.BackInBlack      = CheckAndCut(Request, _r_blackBG);
            _options.ForceCenter      = CheckAndCut(Request, _r_colorPC);
            _options.PickColor        = CheckAndCut(Request, _r_colorPP);
            _options.UseLeftAlignment = CheckAndCut(Request, _r_left   );
            _options.UltraThinCard    = CheckAndCut(Request, _r_thinner);
            _options.ThinCard         = CheckAndCut(Request, _r_thin   );
        }

        protected override string GetMemeText(string? text)
        {
            var generate = text.IsNull_OrEmpty();
            var capitalize = CheckCaps(Request, _r_caps, generate);

            var caption = generate ? Baka.Generate() : text!;

            if (_options.FontOption.IsDefault)
                _options.FontOption.FontKey = caption.IsMostlyCyrillic() ? "sg" : "ft";

            return capitalize ? caption.InLetterCase(LetterCase.Upper) : caption;
        }

        private static readonly Regex
            _r_left    = new(@"^\/top\S*(la)\S*",  RegexOptions.Compiled),
            _r_thinner = new(@"^\/top\S*mm(!)\S*", RegexOptions.Compiled),
            _r_thin    = new(@"^\/top\S*(mm)\S*",  RegexOptions.Compiled),
            _r_colorPP = new(@"^\/top\S*(pp)\S*",  RegexOptions.Compiled),
            _r_colorPC = new(@"^\/top\S*pp(!)\S*", RegexOptions.Compiled),
            _r_blackBG = new(@"^\/top\S*(ob)\S*",  RegexOptions.Compiled),
            _r_crop    = new(@"^\/top\S*?(-?\d{1,2})(%)\S*",     RegexOptions.Compiled),
            _r_fontSM  = new(@"^\/top\S*?(\d{1,3})("")\S*",      RegexOptions.Compiled),
            _r_fontMS  = new(@"^\/top\S*?(min)(\d{1,3})("")\S*", RegexOptions.Compiled);
    }
}