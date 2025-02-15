﻿using Witlesss.Backrooms.Types.SerialQueue;
using Witlesss.Commands.Meme.Core;
using Witlesss.Memes;
using Witlesss.Memes.Shared;
using static Witlesss.Backrooms.Helpers.OptionsParsing;

namespace Witlesss.Commands.Meme
{
    public class Demotivate3000 : MakeMemeCore<string>
    {
        private static readonly DynamicDemotivatorDrawer _dp = new();
        private static readonly SerialTaskQueue _queue = new();

        protected override SerialTaskQueue Queue { get; } = _queue;
        protected override IMemeGenerator<string> MemeMaker => _dp;

        protected override Regex _cmd { get; } = new(@"^\/dp(\S*)");

        protected override string VideoName => "piece_fap_bot-dp.mp4";

        protected override string Log_STR => "DEMOTIVATOR-B";
        protected override string Command => "/dp";
        protected override string Suffix  => "-Dp";

        protected override string? DefaultOptions => Data.Options?.Dp;


        protected override Task Run() => RunInternal("dp");

        protected override bool ResultsAreRandom => DynamicDemotivatorDrawer.FontWizard.UseRandom;

        protected override void ParseOptions()
        {
            DynamicDemotivatorDrawer.MinSizeMultiplier  = GetInt(Request, _fontMS,  10, group: 2);
            DynamicDemotivatorDrawer.FontSizeMultiplier = GetInt(Request, _fontSM, 100);

            DynamicDemotivatorDrawer.CustomColor.CheckAndCut(Request);
            DynamicDemotivatorDrawer.FontWizard .CheckAndCut(Request);

            DynamicDemotivatorDrawer.WrapText   = !CheckAndCut(Request, _nowrap);
            DynamicDemotivatorDrawer.Minimalist =  CheckAndCut(Request, _small);
        }

        protected override string GetMemeText(string? text)
        {
            var generate = string.IsNullOrEmpty(text);
            var caption = generate ? Baka.Generate() : text!;

            var capitalize = CheckCaps(Request, _caps, generate) || generate && caption.Length <= 12;
            return capitalize ? caption.InLetterCase(LetterCase.Upper) : caption;
        }

        private static readonly Regex _small   = new(@"^\/dp\S*(xx)\S*");
        private static readonly Regex _fontSM  = new(@"^\/dp\S*?(\d{1,3})("")\S*");
        private static readonly Regex _fontMS  = new(@"^\/dp\S*?(min)(\d{1,3})("")\S*");
    }
}