using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Witlesss.Backrooms.SerialQueue;
using Witlesss.MediaTools;

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
            DynamicDemotivatorDrawer.ExtraFonts.CheckAndCut(Request);
            DynamicDemotivatorDrawer.CropEdges = OptionsParsing.CheckAndCut(Request, _crop);
        }

        protected override string GetMemeText(string? text)
        {
            var matchCaps = OptionsParsing.Check(Request, _caps);

            var gen = string.IsNullOrEmpty(text);
            var caps = matchCaps && (gen || _caps.IsMatch(Request.Command)); // command, not chat defaults

            var txt = gen ? Baka.Generate() : text!;

            return caps ? txt.ToLetterCase(LetterCaseMode.Upper) : txt;
        }

        private static readonly Regex _crop = new(@"^\/dp\S*cp\S*", RegexOptions.IgnoreCase);
        private static readonly Regex _caps = new(@"^\/dp\S*up\S*", RegexOptions.IgnoreCase);

        // LOGIC

        private static readonly DynamicDemotivatorDrawer  _dp = new();
        private static readonly SerialTaskQueue _queue = new();

        protected override IMemeGenerator<string> MemeMaker => _dp;
        protected override SerialTaskQueue Queue => _queue;
    }
}