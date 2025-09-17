using PF_Bot.Core.Memes.Generators;
using PF_Bot.Core.Memes.Options;
using PF_Bot.Core.Memes.Shared;
using PF_Bot.Handlers.Memes.Core;
using static PF_Bot.Backrooms.Helpers.OptionsParsing;

namespace PF_Bot.Handlers.Memes
{
    public class Demotivate3000 : MakeMemeCore<string>
    {
        private static readonly FontWizard _fontWizard = new ("sg", "dp");
        private static readonly ColorWizard _colorWizard = new ("#");

        private MemeOptions_Dp _options;

        protected override IMemeGenerator<string> MemeMaker => new DynamicDemotivatorDrawer(_options);

        protected override Regex _rgx_cmd { get; } = new(@"^\/dp(\S*)", RegexOptions.Compiled);

        protected override string VideoName => "piece_fap_bot-dp.mp4";

        protected override string Log_STR => "DEMOTIVATOR-B";
        protected override string Command => "/dp";
        protected override string Suffix  =>  "Dp";

        protected override string? DefaultOptions => Data.Options?.Dp;


        protected override Task Run() => RunInternal("dp");

        protected override bool ResultsAreRandom => _options.FontOption.IsRandom;

        protected override void ParseOptions()
        {
            _options.MinSizeMultiplier  = GetInt(Request, _r_fontMS,  10, group: 2);
            _options.FontSizeMultiplier = GetInt(Request, _r_fontSM, 100);

            _options.CustomColor = _colorWizard.CheckAndCut(Request);
            _options.FontOption = _fontWizard.CheckAndCut(Request);

            _options.WrapText   = CheckAndCut(Request, _r_nowrap).Failed();
            _options.Minimalist = CheckAndCut(Request, _r_small);
        }

        protected override string GetMemeText(string? text)
        {
            var generate = text.IsNull_OrEmpty();
            var caption = generate ? Baka.Generate() : text!;

            var capitalize = CheckCaps(Request, _r_caps, generate) || generate && caption.Length <= 12;
            return capitalize ? caption.InLetterCase(LetterCase.Upper) : caption;
        }

        private static readonly Regex
            _r_small   = new(@"^\/dp\S*(xx)\S*",                RegexOptions.Compiled),
            _r_fontSM  = new(@"^\/dp\S*?(\d{1,3})("")\S*",      RegexOptions.Compiled),
            _r_fontMS  = new(@"^\/dp\S*?(min)(\d{1,3})("")\S*", RegexOptions.Compiled);
    }
}