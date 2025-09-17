using PF_Bot.Core.Memes.Generators;
using PF_Bot.Core.Memes.Options;
using PF_Bot.Core.Memes.Shared;

namespace PF_Bot.Handlers.Memes
{
    public class Meme : MakeMemeCore<TextPair>
    {
        private static readonly FontWizard _fontWizard = new ("im");
        private static readonly ColorWizard _colorWizardBack = new ("_");
        private static readonly ColorWizard _colorWizardText = new ("#");

        private MemeOptions_Meme _options;

        protected override IMemeGenerator<TextPair> MemeMaker => new MemeGenerator(_options);

        protected override Regex _rgx_cmd { get; } = new(@"^\/meme(\S*)", RegexOptions.Compiled);

        protected override string VideoName => "piece_fap_bot-meme.mp4";

        protected override string Log_STR => "MEME";
        protected override string Log_CMD => "/meme";
        protected override string Suffix  => "Meme";

        protected override string? DefaultOptions => Data.Options?.Meme;


        protected override Task Run() => RunInternal("meme");

        protected override bool ResultsAreRandom
            => _options.RandomTextColor
            || _options.FontOption.IsRandom
            || Options.Check(_r_add_bottom) && Args!.Contains('\n').Janai(); // (random bottom text)

        protected override void ParseOptions()
        {
            _options.CustomColorBack = _colorWizardBack.CheckAndCut(Options);
            _options.CustomColorText = _colorWizardText.CheckAndCut(Options);

            _options.FontOption = _fontWizard.CheckAndCut(Options);

            _options.FontSizeMultiplier = Options.GetInt(_r_fontSize, 100);
            _options.ShadowOpacity      = Options.GetInt(_r_shadow, 100).ClampByte().Clamp100();
            _options.TextOffset         = Options.GetInt(_r_offset, -1);

            _options.RandomTextOffset   = Options.CheckAndCut(_r_randomOffset);
            _options.WrapText           = Options.CheckAndCut(_r_nowrap).Failed();
            _options.RandomTextColor    = Options.CheckAndCut(_r_colorText);
            _options.AbsolutelyNoMargin = Options.CheckAndCut(_r_noMarginDude);
            _options.NoMargin           = Options.CheckAndCut(_r_noMargin);
        }

        protected override TextPair GetMemeText(string? text)
        {
            var generate = text.IsNull_OrEmpty();
            var capitalize = Options.CheckCaps(_r_caps, generate);

            var lowerCase      = Options.Check(_r_lowerCase);
            var addBottomText  = Options.Check(_r_add_bottom);
            var onlyBottomText = Options.Check(_r_only_bottom);
            var onlyTopText    = Options.Check(_r_top_only);

            string a, b;

            if (_options.FloatingCaptionMode)
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

        private const string
            _r_add_bottom   = "s",
            _r_only_bottom  = "d",
            _r_top_only     = "t",
            _r_lowerCase    = "lo",
            _r_colorText    = "cc",
            _r_randomOffset = "!!",
            _r_noMargin     = "mm",
            _r_noMarginDude = "mm!";

        private static readonly Regex
            _r_fontSize = new(@"(\d{1,3})("")", RegexOptions.Compiled),
            _r_shadow   = new(@"(\d{1,3})(%)",  RegexOptions.Compiled),
            _r_offset   = new(@"(\d{1,3})(!)",  RegexOptions.Compiled);
    }
}