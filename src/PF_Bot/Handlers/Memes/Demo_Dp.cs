using PF_Bot.Core.Memes.Generators;
using PF_Bot.Core.Memes.Options;
using PF_Bot.Core.Memes.Shared;

namespace PF_Bot.Handlers.Memes
{
    public class Demo_Dp : MakeMemeCore<string>
    {
        private static readonly FontWizard _fontWizard = new ("sg");
        private static readonly ColorWizard _colorWizard = new ("#");

        private MemeOptions_Dp _options;

        protected override IMemeGenerator<string> MemeMaker => new DynamicDemotivatorDrawer(_options);

        protected override Regex _rgx_cmd { get; } = new(@"^\/dp(\S*)", RegexOptions.Compiled);

        protected override string VideoName => "piece_fap_bot-dp.mp4";

        protected override string Log_STR => "DEMOTIVATOR-B";
        protected override string Log_CMD => "/dp";
        protected override string Suffix  =>  "Dp";

        protected override string? DefaultOptions => Data.Options?.Dp;


        protected override Task Run() => RunInternal("dp");

        protected override bool ResultsAreRandom => _options.FontOption.IsRandom;

        protected override void ParseOptions()
        {
            _options.MinSizeMultiplier  = Options.GetInt(_r_fontMS,  10, group: 2);
            _options.FontSizeMultiplier = Options.GetInt(_r_fontSM, 100);

            _options.CustomColor = _colorWizard.CheckAndCut(Options);
            _options.FontOption = _fontWizard.CheckAndCut(Options);

            _options.WrapText   = Options.CheckAndCut(_r_nowrap).Failed();
            _options.Minimalist = Options.CheckAndCut(_r_small);
        }

        protected override string GetMemeText(string? text)
        {
            var generate = text.IsNull_OrEmpty();
            var caption = generate ? Baka.Generate() : text!;

            var capitalize = Options.CheckCaps(_r_caps, generate) || generate && caption.Length <= 12;
            return capitalize ? caption.InLetterCase(LetterCase.Upper) : caption;
        }

        private const string
            _r_small  = "xx";

        private static readonly Regex
            _r_fontSM = new(     @"(\d{1,3})("")", RegexOptions.Compiled),
            _r_fontMS = new(@"(min)(\d{1,3})("")", RegexOptions.Compiled);
    }
}