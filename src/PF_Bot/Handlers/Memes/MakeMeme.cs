using PF_Bot.Core.Memes.Generators;
using PF_Bot.Core.Memes.Options;
using PF_Bot.Core.Memes.Shared;
using PF_Bot.Handlers.Memes.Core;
using static PF_Bot.Backrooms.Helpers.OptionsParsing;

namespace PF_Bot.Handlers.Memes
{
    public class MakeMeme : MakeMemeCore<TextPair>
    {
        private static readonly FontWizard _fontWizard = new ("im", "meme");
        private static readonly ColorWizard _colorWizardBack = new ("_");
        private static readonly ColorWizard _colorWizardText = new ("#");

        private MemeOptions_Meme _options;

        protected override IMemeGenerator<TextPair> MemeMaker => new MemeGenerator(_options);

        protected override Regex _rgx_cmd { get; } = new(@"^\/meme(\S*)", RegexOptions.Compiled);

        protected override string VideoName => "piece_fap_bot-meme.mp4";

        protected override string Log_STR => "MEME";
        protected override string Command => "/meme";
        protected override string Suffix  => "Meme";

        protected override string? DefaultOptions => Data.Options?.Meme;


        protected override Task Run() => RunInternal("meme");

        protected override bool ResultsAreRandom
            => _options.RandomTextColor
            || _options.FontOption.IsRandom
            || Check(Request, _r_add_bottom) && Args!.Contains('\n').Janai(); // (random bottom text)

        protected override void ParseOptions()
        {
            _options.CustomColorBack = _colorWizardBack.CheckAndCut(Request);
            _options.CustomColorText = _colorWizardText.CheckAndCut(Request);

            _options.FontOption = _fontWizard.CheckAndCut(Request);

            _options.FontMultiplier = GetInt(Request, _r_fontSM, 100);
            _options.ShadowOpacity  = GetInt(Request, _r_shadow, 100).Clamp(0, 100).ClampByte();
            _options.TextOffset     = GetInt(Request, _r_offset, -1);

            _options.RandomOffset       = CheckAndCut(Request, _r_randomOffset);
            _options.WrapText           = CheckAndCut(Request, _r_nowrap).Failed();
            _options.RandomTextColor    = CheckAndCut(Request, _r_colorText);
            _options.AbsolutelyNoMargin = CheckAndCut(Request, _r_noMarginDude);
            _options.NoMargin           = CheckAndCut(Request, _r_noMargin);
        }

        protected override TextPair GetMemeText(string? text)
        {
            var generate = text.IsNull_OrEmpty();
            var capitalize = CheckCaps(Request, _r_caps, generate);

            var lowerCase      = Check(Request,   _r_lowerCase);
            var addBottomText  = Check(Request,  _r_add_bottom);
            var onlyBottomText = Check(Request, _r_only_bottom);
            var onlyTopText    = Check(Request,    _r_top_only);

            string a, b;

            if (_options.CustomOffsetMode)
            {
                a = generate ? Baka.Generate() : text!;
                b = "";
            }
            else if (generate)
            {
                var (genA, genB) = (true, true);

                var chance = Random.Shared.Next(6);

                if /**/ (onlyBottomText) genA = false;
                else if (onlyTopText)    genB = false;
                else if (chance == 0)    genA = false;
                else if (chance == 1)    genB = false;

                a = genA ? Baka.Generate() : "";
                b = genB ? Baka.Generate() : "";

                if (genA && onlyTopText.Janai() && (genB ? a.Length > b.Length : a.Length > 64))
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

        private static readonly Regex
            _r_add_bottom   = new(@"^\/meme\S*(s)\S*",   RegexOptions.Compiled),
            _r_only_bottom  = new(@"^\/meme\S*(d)\S*",   RegexOptions.Compiled),
            _r_top_only     = new(@"^\/meme\S*(t)\S*",   RegexOptions.Compiled),
            _r_lowerCase    = new(@"^\/meme\S*(lo)\S*",  RegexOptions.Compiled),
            _r_colorText    = new(@"^\/meme\S*(cc)\S*",  RegexOptions.Compiled),
            _r_randomOffset = new(@"^\/meme\S*(!!)\S*",  RegexOptions.Compiled),
            _r_noMargin     = new(@"^\/meme\S*(mm)\S*",  RegexOptions.Compiled),
            _r_noMarginDude = new(@"^\/meme\S*(mm!)\S*", RegexOptions.Compiled),
            _r_fontSM       = new(@"^\/meme\S*?(\d{1,3})("")\S*", RegexOptions.Compiled),
            _r_shadow       = new(@"^\/meme\S*?(\d{1,3})(%)\S*",  RegexOptions.Compiled),
            _r_offset       = new(@"^\/meme\S*?(\d{1,3})(!)\S*",  RegexOptions.Compiled);
    }
}