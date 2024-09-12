using Witlesss.Commands.Meme.Core;
using Witlesss.Memes;
using Witlesss.Memes.Shared;
using static Witlesss.Backrooms.Helpers.OptionsParsing;

namespace Witlesss.Commands.Meme
{
    public class Top : MakeMemeCore<string>
    {
        private static readonly IFunnyApp _ifunny = new();

        protected override IMemeGenerator<string> MemeMaker => _ifunny;

        protected override Regex _cmd { get; } = new(@"^\/top(\S*)");

        protected override string VideoName => "piece_fap_bot-top.mp4";

        protected override string Log_STR => "TOP";
        protected override string Command => "/top";
        protected override string Suffix  => "-Top";

        protected override string? DefaultOptions => Baka.Options?.Top;


        protected override Task Run() => RunInternal("подписанки", "top"); // 🔥🔥🔥✍️

        protected override bool ResultsAreRandom => IFunnyApp.ExtraFonts.UseRandom;

        protected override void ParseOptions()
        {
            IFunnyApp.CustomColor.CheckAndCut(Request);
            IFunnyApp.ExtraFonts .CheckAndCut(Request);

            IFunnyApp.CropPercent        = GetInt(Request, _crop,     0);
            IFunnyApp.MinSizeMultiplier  = GetInt(Request, _fontMS,  10, group: 2);
            IFunnyApp.FontSizeMultiplier = GetInt(Request, _fontSM, 100);

            IFunnyApp.WrapText         = !CheckAndCut(Request, _nowrap );
            IFunnyApp.BackInBlack      =  CheckAndCut(Request, _blackBG);
            IFunnyApp.ForceCenter      =  CheckAndCut(Request, _colorPC);
            IFunnyApp.PickColor        =  CheckAndCut(Request, _colorPP);
            IFunnyApp.UseLeftAlignment =  CheckAndCut(Request, _left   );
            IFunnyApp.UltraThinCard    =  CheckAndCut(Request, _thinner);
            IFunnyApp.ThinCard         =  CheckAndCut(Request, _thin   );
        }

        protected override string GetMemeText(string? text)
        {
            var generate = string.IsNullOrEmpty(text);
            var capitalize = CheckCaps(Request, _caps, generate);

            var caption = generate ? Baka.Generate() : text!;

            IFunnyApp.PreferSegoe = caption.IsMostlyCyrillic();

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