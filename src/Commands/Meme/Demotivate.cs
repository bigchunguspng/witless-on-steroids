using Witlesss.Backrooms.Types.SerialQueue;
using Witlesss.Commands.Meme.Core;
using Witlesss.Memes;
using Witlesss.Memes.Shared;
using static Witlesss.Commands.Meme.Demotivate.Mode;

namespace Witlesss.Commands.Meme
{
    public class Demotivate : MakeMemeCore<TextPair>
    {
        protected override Regex _cmd { get; } = new(@"^\/d[vg](\S*)");

        protected override string VideoName => $"piece_fap_club-d{(_mode == Square ? "g" : "v")}.mp4";

        protected override string Log_STR => "DEMOTIVATOR";
        protected override string Command => _mode == Square ? "/dg" : "/dv";
        protected override string Suffix  => _mode == Square ? "-Dg" : "-Dv";

        protected override string? DefaultOptions => Baka.Options?.Dg;


        protected override Task Run() => RunInternal("демотиваторы💀", "dg");

        protected override bool ResultsAreRandom => DemotivatorDrawer.AddLogo || RandomFontIsUsed;

        private bool RandomFontIsUsed
            => _mode == Wide
                ? DemotivatorDrawer.ExtraFontsL.UseRandom
                : DemotivatorDrawer.SingleLine
                    ? DemotivatorDrawer.ExtraFontsS.UseRandom
                    : DemotivatorDrawer.ExtraFontsA.UseRandom
                   || DemotivatorDrawer.ExtraFontsB.UseRandom;

        protected override void ParseOptions()
        {
            DemotivatorDrawer.SingleLine = OptionsParsing.CheckAndCut(Request, _one_line);

            if (_mode == Wide)
            {
                DemotivatorDrawer.ExtraFontsL.CheckAndCut(Request);
            }
            else if (DemotivatorDrawer.SingleLine)
            {
                DemotivatorDrawer.ExtraFontsS.CheckAndCut(Request);
            }
            else
            {
                DemotivatorDrawer.ExtraFontsA.CheckAndCut(Request);
                DemotivatorDrawer.ExtraFontsB.CheckAndCut(Request);
            }

            DemotivatorDrawer.AddLogo = !OptionsParsing.Check(Request, _no_logo);
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

            var capitalize = OptionsParsing.CheckCaps(Request, _caps, generate);
            if (capitalize) a = a.InLetterCase(LetterCase.Upper);

            return new TextPair(a, b);
        }

        private static readonly Regex _no_logo  = new(@"^\/d[vg]\S*(nn)\S* *");
        private static readonly Regex _one_line = new(@"^\/d[vg]\S*(ll)\S* *");

        public Demotivate SetMode(Mode mode)
        {
            _mode = mode;
            return this;
        }

        public void SelectMode(float w, float h) => _mode = w / h > 1.6 ? Wide : Square;

        // LOGIC

        private Mode _mode;

        private static readonly SerialTaskQueue _queue = new();
        private static readonly DemotivatorDrawer[] _drawers =
        [
            new DemotivatorDrawer(), new DemotivatorDrawer(1280)
        ];

        protected override IMemeGenerator<TextPair> MemeMaker => _drawers[(int) _mode];
        protected override SerialTaskQueue Queue => _queue;

        public enum Mode
        {
            Square, Wide
        }
    }
}