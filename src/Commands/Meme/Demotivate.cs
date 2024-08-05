using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Witlesss.Backrooms.Helpers;
using Witlesss.Backrooms.SerialQueue;
using Witlesss.Commands.Meme.Core;
using Witlesss.MediaTools;
using Witlesss.Memes;
using Witlesss.Memes.Shared;
using static Witlesss.XD.DgMode;

namespace Witlesss.Commands.Meme
{
    public class Demotivate : MakeMemeCore<TextPair>, ImageProcessor
    {
        protected override Regex _cmd { get; } = new(@"^\/d[vg](\S*) *", RegexOptions.IgnoreCase);

        protected override string Log_PHOTO(int x) => $"DEMOTIVATOR [{(x == 1 ? "_" : x)}]";
        protected override string Log_STICK(int x) => $"DEMOTIVATOR [{(x == 1 ? "#" : x)}] STICKER";

        protected override string Log_VIDEO => "DEMOTIVATOR [^] VID";
        protected override string VideoName => $"piece_fap_club-d{(_mode == Square ? "g" : "v")}.mp4";

        protected override string Command => _mode == Square ? "/dg" : "/dv";
        protected override string Suffix  => _mode == Square ? "-Dg" : "-Dv";

        protected override string? DefaultOptions => Baka.Options?.Dg;


        protected override Task Run() => RunInternal("Демотиваторы💀");

        protected override bool ResultsAreRandom => DemotivatorDrawer.AddLogo;

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
            if (capitalize) a = a.ToLetterCase(LetterCaseMode.Upper);

            return new TextPair(a, b);
        }

        private static readonly Regex _no_logo  = new(@"^\/d[vg]\S*(nn)\S* *");
        private static readonly Regex _one_line = new(@"^\/d[vg]\S*(ll)\S* *");

        public Demotivate SetMode(DgMode mode)
        {
            _mode = mode;
            return this;
        }

        public void SelectMode(float w, float h) => _mode = w / h > 1.6 ? Wide : Square;

        // LOGIC

        private DgMode _mode;

        private static readonly SerialTaskQueue _queue = new();
        private static readonly DemotivatorDrawer[] _drawers =
        [
            new DemotivatorDrawer(), new DemotivatorDrawer(1280)
        ];

        protected override IMemeGenerator<TextPair> MemeMaker => _drawers[(int) _mode];
        protected override SerialTaskQueue Queue => _queue;
    }
}