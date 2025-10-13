using PF_Bot.Features_Main.Memes.Core.Generators;
using PF_Bot.Features_Main.Memes.Core.Options;
using PF_Bot.Features_Main.Memes.Core.Shared;

namespace PF_Bot.Features_Main.Memes.Commands
{
    public class Demo_Dp : Meme_Core<string>
    {
        private static readonly FontWizard _fontWizard = new ("sg");
        private static readonly ColorWizard _colorWizard = new ("#");

        private MemeOptions_Dp _options;

        protected override IMemeGenerator<string> MemeMaker => new Demotivators2077(_options);

        protected override string VideoName => "piece_fap_bot-dp.mp4";

        protected override string Log_STR => "DEMOTIVATOR-B";
        protected override string Log_CMD => "/dp";
        protected override string Suffix  =>  "Dp";

        protected override string? DefaultOptions => Data.Options?.Dp;


        protected override Task Run() => RunInternal("dp");

        protected override bool ResultsAreRandom => _options.FontOption.IsRandom;

        protected override void ParseOptions()
        {
            _options.MinFontSizeMultiplier = MemeOptions.GetInt(_r_fontSizeMin,  10, group: 2);
            _options.   FontSizeMultiplier = MemeOptions.GetInt(_r_fontSize, 100);

            _options.CustomColor = _colorWizard.CheckAndCut(MemeOptions);
            _options.FontOption  =  _fontWizard.CheckAndCut(MemeOptions);

            _options.WrapText   = MemeOptions.CheckAndCut(_r_nowrap).Failed();
            _options.Minimalist = MemeOptions.CheckAndCut(_r_small);
        }

        protected override string GetMemeText(string? text)
        {
            var generate = text.IsNull_OrEmpty();
            var caption = generate ? Baka.Generate() : text!;

            var capitalize = MemeOptions.CheckCaps(_r_caps, generate) || generate && caption.Length <= 12;
            return capitalize ? caption.InLetterCase(LetterCase.Upper) : caption;
        }

        private const string
            _r_small  = "xx";

        private static readonly Regex
            _r_fontSize    = new(     @"(\d{1,3})("")", RegexOptions.Compiled),
            _r_fontSizeMin = new(@"(min)(\d{1,3})("")", RegexOptions.Compiled);
    }
}