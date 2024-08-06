using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Witlesss.Backrooms.SerialQueue;
using Witlesss.Commands.Meme.Core;
using Witlesss.Memes;
using Witlesss.Memes.Shared;
using static Witlesss.Backrooms.Helpers.OptionsParsing;

namespace Witlesss.Commands.Meme
{
    public class Demotivate3000 : MakeMemeCore<string>, ImageProcessor
    {
        protected override Regex _cmd { get; } = new(@"^\/dp(\S*) *", RegexOptions.IgnoreCase);

        protected override string Log_PHOTO(int x) => $"DEMOTIVATOR-B [{(x == 1 ? "_" : x)}]";
        protected override string Log_STICK(int x) => $"DEMOTIVATOR-B [{(x == 1 ? "#" : x)}] STICKER";

        protected override string Log_VIDEO => "DEMOTIVATOR-B [^] VID";
        protected override string VideoName => "piece_fap_club-dp.mp4";
        
        protected override string Command => "/dp";
        protected override string Suffix  => "-Dp";

        protected override string? DefaultOptions => Baka.Options?.Dp;


        protected override Task Run() => RunInternal("демотиваторы👌", "dp");

        protected override void ParseOptions()
        {
            DynamicDemotivatorDrawer.CustomColorOption.CheckAndCut(Request);
            DynamicDemotivatorDrawer.ExtraFonts.CheckAndCut(Request);

            DynamicDemotivatorDrawer.WrapText   = !CheckAndCut(Request, _nowrap);
            DynamicDemotivatorDrawer.Minimalist =  CheckAndCut(Request, _small);
        }

        protected override string GetMemeText(string? text)
        {
            var generate = string.IsNullOrEmpty(text);
            var caption = generate ? Baka.Generate() : text!;

            var capitalize = CheckCaps(Request, _caps, generate) || generate && caption.Length <= 12;
            return capitalize ? caption.ToLetterCase(LetterCaseMode.Upper) : caption;
        }

        private static readonly Regex _small = new(@"^\/dp\S*(xx)\S*");

        // LOGIC

        private static readonly DynamicDemotivatorDrawer  _dp = new();
        private static readonly SerialTaskQueue _queue = new();

        protected override IMemeGenerator<string> MemeMaker => _dp;
        protected override SerialTaskQueue Queue => _queue;
    }
}