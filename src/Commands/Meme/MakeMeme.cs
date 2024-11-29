using System.Diagnostics;
using Witlesss.Backrooms.Types.SerialQueue;
using Witlesss.Commands.Meme.Core;
using Witlesss.Memes;
using Witlesss.Memes.Shared;
using static Witlesss.Backrooms.Helpers.OptionsParsing;

namespace Witlesss.Commands.Meme
{
    public class MakeMeme : MakeMemeCore<TextPair>
    {
        private static readonly MemeGenerator _imgflip = new();
        private static readonly SerialTaskQueue _queue = new();

        protected override SerialTaskQueue Queue { get; } = _queue;
        protected override IMemeGenerator<TextPair> MemeMaker => _imgflip;

        protected override Regex _cmd { get; } = new(@"^\/meme(\S*)");

        protected override string VideoName => "piece_fap_bot-meme.mp4";

        protected override string Log_STR => "MEME";
        protected override string Command => "/meme";
        protected override string Suffix  => "-Meme";

        protected override string? DefaultOptions => Data.Options?.Meme;


        protected override Task Run() => RunInternal("meme");

        protected override bool ResultsAreRandom
            => MemeGenerator.RandomTextColor || MemeGenerator.FontWizard.UseRandom;

        protected override void ParseOptions()
        {
            MemeGenerator.CustomColorBack.CheckAndCut(Request);
            MemeGenerator.CustomColorText.CheckAndCut(Request);
            MemeGenerator.FontWizard     .CheckAndCut(Request);

            MemeGenerator.FontMultiplier =  GetInt(Request, _fontSM, 100);
            MemeGenerator.ShadowOpacity  =  GetInt(Request, _shadow, 100).Clamp(0, 100);

            MemeGenerator.WrapText  = !CheckAndCut(Request, _nowrap);
            MemeGenerator.RandomTextColor =  CheckAndCut(Request, _colorText);
        }

        protected override TextPair GetMemeText(string? text)
        {
            var generate = text.IsNullOrEmpty();
            var capitalize = CheckCaps(Request, _caps, generate);

            var lowerCase      = Check(Request,   _lowerCase);
            var addBottomText  = Check(Request,  _add_bottom);
            var onlyBottomText = Check(Request, _only_bottom);
            var onlyTopText    = Check(Request,    _top_only);

            string a, b;

            if (generate)
            {
                var (genA, genB) = (true, true);

                var chance = Random.Shared.Next(6);

                if /**/ (onlyBottomText) genA = false;
                else if (onlyTopText)    genB = false;
                else if (chance == 0)    genA = false;
                else if (chance == 1)    genB = false;

                a = genA ? Baka.Generate() : "";
                b = genB ? Baka.Generate() : "";

                if (genA && !onlyTopText && (genB ? a.Length > b.Length : a.Length > 64))
                {
                    // bigger text (or big enough one) should be at the bottom
                    (a, b) = (b, a);
                }
            }
            else
            {
                Debug.Assert(text != null);

                var separator = TryGetSeparator(text);
                if (separator is not null)
                {
                    var s = text.Split(separator, 2);
                    (a, b) = (s[0], s[1]);
                }
                else
                {
                    a = text;
                    b = addBottomText ? Baka.Generate() : "";
                }
            }

            return new TextPair(AdjustCase(a), AdjustCase(b));

            string AdjustCase
                (string s) => lowerCase ? s.ToLower() : generate || capitalize ? s.ToUpper() : s;
        }

        private static readonly string[] separators = ["\n\n\n\n", "\n\n\n", "\n\n", "\n"];

        private static string? TryGetSeparator(string text)
        {
            return separators.FirstOrDefault(text.Contains);
        }

        private static readonly Regex  _add_bottom = new(@"^\/meme\S*(s)\S*");
        private static readonly Regex _only_bottom = new(@"^\/meme\S*(d)\S*");
        private static readonly Regex    _top_only = new(@"^\/meme\S*(t)\S*");
        private static readonly Regex   _lowerCase = new(@"^\/meme\S*(lo)\S*");
        private static readonly Regex   _colorText = new(@"^\/meme\S*(cc)\S*");
        private static readonly Regex      _fontSM = new(@"^\/meme\S*?(\d{1,3})("")\S*");
        private static readonly Regex      _shadow = new(@"^\/meme\S*?(\d{1,3})(%)\S*");
    }
}