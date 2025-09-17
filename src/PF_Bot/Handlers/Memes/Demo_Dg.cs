using PF_Bot.Core.Memes.Generators;
using PF_Bot.Core.Memes.Options;
using PF_Bot.Core.Memes.Shared;
using static PF_Bot.Handlers.Memes.Demo_Dg.Mode;

namespace PF_Bot.Handlers.Memes
{
    public class Demo_Dg : MakeMemeCore<TextPair>
    {
        private static readonly FontWizard
            FontWizardL = new("ro", "(?![-bi*])"),
            FontWizardS = new("ro", "(?![-bi*])"),
            FontWizardA = new("ro",   "(&)"),
            FontWizardB = new("co", @"(\*)");

        private MemeOptions_Dg _options;

        protected override IMemeGenerator<TextPair> MemeMaker =>
            _mode == Square
                ? new DemotivatorDrawer(_options)
                : new DemotivatorDrawer(_options, 1280);

        protected override Regex _rgx_cmd { get; } = new(@"^\/d[vg](\S*)", RegexOptions.Compiled);

        protected override string VideoName => $"piece_fap_bot-d{(_mode == Square ? "g" : "v")}.mp4";

        protected override string Log_STR => "DEMOTIVATOR";
        protected override string Log_CMD => _mode == Square ? "/dg" : "/dv";
        protected override string Suffix  => _mode == Square ?  "Dg" :  "Dv";

        protected override string? DefaultOptions => Data.Options?.Dg;


        protected override Task Run() => RunInternal("dg");

        protected override bool ResultsAreRandom
            => _options.AddLogo
            || RandomFontIsUsed
            || _options.SingleLine.IsOff() && Args!.Contains('\n').Janai(); // (random bottom text)

        private bool RandomFontIsUsed
            => _mode == Wide || _options.SingleLine
                ? _options.FontOptionA.IsRandom
                : _options.FontOptionA.IsRandom
               || _options.FontOptionB.IsRandom;

        protected override void ParseOptions()
        {
            _options.SingleLine = Options.CheckAndCut(_r_one_line);

            if (_mode == Wide)
            {
                _options.FontOptionA = FontWizardL.CheckAndCut(Options);
            }
            else if (_options.SingleLine)
            {
                _options.FontOptionA = FontWizardS.CheckAndCut(Options);
            }
            else
            {
                _options.FontOptionA = FontWizardA.CheckAndCut(Options);
                _options.FontOptionB = FontWizardB.CheckAndCut(Options);
            }

            _options.AddLogo = Options.CheckAndCut(_r_no_logo).Failed();
        }

        protected override TextPair GetMemeText(string? text)
        {
            _options.BottomTextIsGenerated = true;

            string a, b;
            var generate = text.IsNull_OrEmpty();
            if (generate)
            {
                a = Baka.Generate();
                b = Baka.Generate().EnsureIsNotUppercase();
            }
            else
            {
                if (text!.Contains('\n'))
                {
                    var split = text.Split('\n', 2);
                    a = split[0];
                    b = split[1];

                    _options.BottomTextIsGenerated = false;
                }
                else
                {
                    a = text;
                    b = Baka.Generate().EnsureIsNotUppercase();
                }
            }

            var capitalize = Options.CheckCaps(_r_caps, generate);
            if (capitalize) a = a.InLetterCase(LetterCase.Upper);

            return new TextPair(a, b);
        }

        private const string
            _r_no_logo  = "nn",
            _r_one_line = "ll";


        // MODE

        public Demo_Dg SetMode(Mode mode)
        {
            _mode = mode;
            return this;
        }

        public void SelectMode(float w, float h) => _mode = w / h > 1.6 ? Wide : Square;

        private Mode _mode;

        public enum Mode
        {
            Square, Wide,
        }
    }
}