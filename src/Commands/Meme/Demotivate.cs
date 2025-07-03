using Witlesss.Backrooms.Types.SerialQueue;
using Witlesss.Commands.Meme.Core;
using Witlesss.Memes;
using Witlesss.Memes.Shared;
using static Witlesss.Backrooms.Helpers.OptionsParsing;
using static Witlesss.Commands.Meme.Demotivate.Mode;

namespace Witlesss.Commands.Meme
{
    public class Demotivate : MakeMemeCore<TextPair>
    {
        private static readonly DemotivatorDrawer[] _drawers = [new DemotivatorDrawer(), new DemotivatorDrawer(1280)];
        private static readonly SerialTaskQueue  [] _queues  = [new SerialTaskQueue(), new SerialTaskQueue()];

        protected override SerialTaskQueue          Queue     => _queues [(int)_mode];
        protected override IMemeGenerator<TextPair> MemeMaker => _drawers[(int)_mode];

        protected override Regex _cmd { get; } = new(@"^\/d[vg](\S*)");

        protected override string VideoName => $"piece_fap_bot-d{(_mode == Square ? "g" : "v")}.mp4";

        protected override string Log_STR => "DEMOTIVATOR";
        protected override string Command => _mode == Square ? "/dg" : "/dv";
        protected override string Suffix  => _mode == Square ? "-Dg" : "-Dv";

        protected override string? DefaultOptions => Data.Options?.Dg;


        protected override Task Run() => RunInternal("dg");

        protected override bool ResultsAreRandom
            => DemotivatorDrawer.AddLogo
            || RandomFontIsUsed
            || !Check(Request, _one_line) && !Args!.Contains('\n');

        private bool RandomFontIsUsed
            => _mode == Wide
                ? DemotivatorDrawer.FontWizardL.UseRandom
                : DemotivatorDrawer.SingleLine
                    ? DemotivatorDrawer.FontWizardS.UseRandom
                    : DemotivatorDrawer.FontWizardA.UseRandom
                   || DemotivatorDrawer.FontWizardB.UseRandom;

        protected override void ParseOptions()
        {
            DemotivatorDrawer.SingleLine = CheckAndCut(Request, _one_line);

            if (_mode == Wide)
            {
                DemotivatorDrawer.FontWizardL.CheckAndCut(Request);
            }
            else if (DemotivatorDrawer.SingleLine)
            {
                DemotivatorDrawer.FontWizardS.CheckAndCut(Request);
            }
            else
            {
                DemotivatorDrawer.FontWizardA.CheckAndCut(Request);
                DemotivatorDrawer.FontWizardB.CheckAndCut(Request);
            }

            DemotivatorDrawer.AddLogo = !Check(Request, _no_logo);
        }

        protected override TextPair GetMemeText(string? text)
        {
            DemotivatorDrawer.BottomTextIsGenerated = true;

            string a, b;
            var generate = string.IsNullOrEmpty(text);
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

                    DemotivatorDrawer.BottomTextIsGenerated = false;
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