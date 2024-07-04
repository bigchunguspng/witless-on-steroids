using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Witlesss.Backrooms.SerialQueue;
using Witlesss.Memes;
using static Witlesss.Backrooms.Helpers.OptionsParsing;

namespace Witlesss.Commands.Meme
{
    public class DemotivateProportional : MakeMemeCore<string>, ImageProcessor
    {
        protected override Regex _cmd { get; } = new(@"^\/dp(\S*) *", RegexOptions.IgnoreCase);

        protected override string Log_PHOTO(int x) => $"DEMOTIVATOR-B [{(x == 1 ? "_" : x)}]";
        protected override string Log_STICK(int x) => $"DEMOTIVATOR-B [{(x == 1 ? "#" : x)}] STICKER";

        protected override string Log_VIDEO { get; } = "DEMOTIVATOR-B [^] VID";
        protected override string VideoName { get; } = "piece_fap_club-dp.mp4";
        
        protected override string Command { get; } = "/dp";
        protected override string Suffix  { get; } = "-Dp";

        protected override string? DefaultOptions => Baka.Meme.Options?.Dp;


        protected override Task Run() => RunInternal("Демотиваторы👌", DP_OPTIONS);

        protected override void ParseOptions()
        {
            DynamicDemotivatorDrawer.CustomColorOption.CheckAndCut(Request);

            DynamicDemotivatorDrawer.WrapText  = !CheckAndCut(Request, _nowrap);
            DynamicDemotivatorDrawer.CropEdges =  CheckAndCut(Request, _crop);

            DynamicDemotivatorDrawer.ExtraFonts.CheckAndCut(Request);
        }

        protected override string GetMemeText(string? text)
        {
            var generate = string.IsNullOrEmpty(text);
            var capitalize = CheckCaps(Request, _caps, generate);

            var caption = generate ? Baka.Generate() : text!;

            return capitalize ? caption.ToLetterCase(LetterCaseMode.Upper) : caption;
        }

        private static readonly Regex _crop = new(@"^\/dp\S*(cp)\S*");

        // LOGIC

        private static readonly DynamicDemotivatorDrawer  _dp = new();
        private static readonly SerialTaskQueue _queue = new();

        protected override IMemeGenerator<string> MemeMaker => _dp;
        protected override SerialTaskQueue Queue => _queue;
    }
}