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

        protected override Regex _cmd { get; } = new(@"^\/top(\S*)");

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

            _options.CropPercent        = GetInt(Request, _crop,     0);
            _options.MinSizeMultiplier  = GetInt(Request, _fontMS,  10, group: 2);
            _options.FontSizeMultiplier = GetInt(Request, _fontSM, 100);

            _options.WrapText         = CheckAndCut(Request, _nowrap ).Failed();
            _options.BackInBlack      = CheckAndCut(Request, _blackBG);
            _options.ForceCenter      = CheckAndCut(Request, _colorPC);
            _options.PickColor        = CheckAndCut(Request, _colorPP);
            _options.UseLeftAlignment = CheckAndCut(Request, _left   );
            _options.UltraThinCard    = CheckAndCut(Request, _thinner);
            _options.ThinCard         = CheckAndCut(Request, _thin   );
        }

        protected override string GetMemeText(string? text)
        {
            var generate = text.IsNull_OrEmpty();
            var capitalize = CheckCaps(Request, _caps, generate);

            var caption = generate ? Baka.Generate() : text!;

            if (_options.FontOption.IsDefault)
                _options.FontOption.FontKey = caption.IsMostlyCyrillic() ? "sg" : "ft";

            return capitalize ? caption.InLetterCase(LetterCase.Upper) : caption;
        }

        private static readonly Regex _left    = new(@"^\/top\S*(la)\S*");
        private static readonly Regex _thinner = new(@"^\/top\S*mm(!)\S*");
        private static readonly Regex _thin    = new(@"^\/top\S*(mm)\S*");
        private static readonly Regex _colorPP = new(@"^\/top\S*(pp)\S*");
        private static readonly Regex _colorPC = new(@"^\/top\S*pp(!)\S*");
        private static readonly Regex _blackBG = new(@"^\/top\S*(ob)\S*");
        private static readonly Regex _crop    = new(@"^\/top\S*?(-?\d{1,2})(%)\S*");
        private static readonly Regex _fontSM  = new(@"^\/top\S*?(\d{1,3})("")\S*");
        private static readonly Regex _fontMS  = new(@"^\/top\S*?(min)(\d{1,3})("")\S*");
    }
}