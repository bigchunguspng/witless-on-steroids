using PF_Bot.Backrooms.Types.SerialQueue;
using PF_Bot.Core.Memes.Generators;
using PF_Bot.Core.Memes.Shared;
using PF_Bot.Features.Generate.Memes.Core;
using PF_Tools.Backrooms.Helpers;
using static PF_Bot.Backrooms.Helpers.OptionsParsing;

namespace PF_Bot.Features.Generate.Memes
{
    public class Top : MakeMemeCore<string>
    {
        private static readonly IFunnyBrazil _ifunny = new();
        private static readonly SerialTaskQueue _queue = new();

        protected override SerialTaskQueue Queue { get; } = _queue;
        protected override IMemeGenerator<string> MemeMaker => _ifunny;

        protected override Regex _cmd { get; } = new(@"^\/top(\S*)");

        protected override string VideoName => "piece_fap_bot-top.mp4";

        protected override string Log_STR => "TOP";
        protected override string Command => "/top";
        protected override string Suffix  => "Top";

        protected override string? DefaultOptions => Data.Options?.Top;


        protected override Task Run() => RunInternal("top");

        protected override bool ResultsAreRandom => IFunnyBrazil.FontWizard.UseRandom;

        protected override void ParseOptions()
        {
            IFunnyBrazil.CustomColor.CheckAndCut(Request);
            IFunnyBrazil.FontWizard .CheckAndCut(Request);

            IFunnyBrazil.CropPercent        = GetInt(Request, _crop,     0);
            IFunnyBrazil.MinSizeMultiplier  = GetInt(Request, _fontMS,  10, group: 2);
            IFunnyBrazil.FontSizeMultiplier = GetInt(Request, _fontSM, 100);

            IFunnyBrazil.WrapText         = !CheckAndCut(Request, _nowrap );
            IFunnyBrazil.BackInBlack      =  CheckAndCut(Request, _blackBG);
            IFunnyBrazil.ForceCenter      =  CheckAndCut(Request, _colorPC);
            IFunnyBrazil.PickColor        =  CheckAndCut(Request, _colorPP);
            IFunnyBrazil.UseLeftAlignment =  CheckAndCut(Request, _left   );
            IFunnyBrazil.UltraThinCard    =  CheckAndCut(Request, _thinner);
            IFunnyBrazil.ThinCard         =  CheckAndCut(Request, _thin   );
        }

        protected override string GetMemeText(string? text)
        {
            var generate = string.IsNullOrEmpty(text);
            var capitalize = CheckCaps(Request, _caps, generate);

            var caption = generate ? Baka.Generate() : text!;

            IFunnyBrazil.PreferSegoe = caption.IsMostlyCyrillic();

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