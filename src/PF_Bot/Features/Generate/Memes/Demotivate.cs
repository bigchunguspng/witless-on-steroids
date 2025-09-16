using PF_Bot.Core.Meme.Generators;
using PF_Bot.Core.Meme.Options;
using PF_Bot.Core.Meme.Shared;
using PF_Bot.Features.Generate.Memes.Core;
using static PF_Bot.Backrooms.Helpers.OptionsParsing;
using static PF_Bot.Features.Generate.Memes.Demotivate.Mode;

namespace PF_Bot.Features.Generate.Memes
{
    public class Demotivate : MakeMemeCore<TextPair>
    {
        private static readonly FontWizard FontWizardL = new("ro", "d[vg]", "(?![-bi*])");
        private static readonly FontWizard FontWizardS = new("ro", "dg",    "(?![-bi*])");
        private static readonly FontWizard FontWizardA = new("ro", "dg",   "(&)");
        private static readonly FontWizard FontWizardB = new("co", "dg", @"(\*)");

        private MemeOptions_Dg _options;

        protected override IMemeGenerator<TextPair> MemeMaker =>
            _mode == Square
                ? new DemotivatorDrawer(_options)
                : new DemotivatorDrawer(_options, 1280);

        protected override Regex _cmd { get; } = new(@"^\/d[vg](\S*)");

        protected override string VideoName => $"piece_fap_bot-d{(_mode == Square ? "g" : "v")}.mp4";

        protected override string Log_STR => "DEMOTIVATOR";
        protected override string Command => _mode == Square ? "/dg" : "/dv";
        protected override string Suffix  => _mode == Square ?  "Dg" :  "Dv";

        protected override string? DefaultOptions => Data.Options?.Dg;


        protected override Task Run() => RunInternal("dg");

        protected override bool ResultsAreRandom
            => _options.AddLogo
            || RandomFontIsUsed
            || Check(Request, _one_line).Failed() && Args!.Contains('\n').Janai(); // (random bottom text)

        private bool RandomFontIsUsed
            => _mode == Wide || _options.SingleLine
                ? _options.FontOptionA.IsRandom
                : _options.FontOptionA.IsRandom
               || _options.FontOptionB.IsRandom;

        protected override void ParseOptions()
        {
            _options.SingleLine = CheckAndCut(Request, _one_line);

            if (_mode == Wide)
            {
                _options.FontOptionA = FontWizardL.CheckAndCut(Request);
            }
            else if (_options.SingleLine)
            {
                _options.FontOptionA = FontWizardS.CheckAndCut(Request);
            }
            else
            {
                _options.FontOptionA = FontWizardA.CheckAndCut(Request);
                _options.FontOptionB = FontWizardB.CheckAndCut(Request);
            }

            _options.AddLogo = Check(Request, _no_logo).Failed();
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

            var capitalize = CheckCaps(Request, _caps, generate);
            if (capitalize) a = a.InLetterCase(LetterCase.Upper);

            return new TextPair(a, b);
        }

        private static readonly Regex _no_logo  = new(@"^\/d[vg]\S*(nn)\S* *");
        private static readonly Regex _one_line = new(@"^\/d[vg]\S*(ll)\S* *");


        // MODE

        public Demotivate SetMode(Mode mode)
        {
            _mode = mode;
            return this;
        }

        public void SelectMode(float w, float h) => _mode = w / h > 1.6 ? Wide : Square;

        private Mode _mode;

        public enum Mode
        {
            Square, Wide
        }
    }
}