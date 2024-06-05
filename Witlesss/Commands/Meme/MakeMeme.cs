using System;
using System.Drawing;
using System.Text.RegularExpressions;
using static Witlesss.Backrooms.OptionsParsing;

namespace Witlesss.Commands.Meme
{
    public class MakeMeme : MakeMemeCore<DgText>, ImageProcessor
    {
        protected override Regex _cmd { get; } = new(@"^\/meme(\S*) *", RegexOptions.IgnoreCase);

        protected override string Log_PHOTO(int x) => $"MEME [{(x == 1 ? "M" : x)}]";
        protected override string Log_STICK(int x) => $"MEME [{(x == 1 ? "M" : x)}] STICKER";

        protected override string Log_VIDEO { get; } = "MEME [M] VID";
        protected override string VideoName { get; } = "piece_fap_club-meme.mp4";

        protected override string Command { get; } = "/meme";

        protected override string? DefaultOptions => Baka.Meme.OptionsM;

        public ImageProcessor SetUp(int w, int h)
        {
            ImageSaver.PassQuality(Baka);

            return this;
        }

        public override void Run() => Run("Мемы", OPTIONS + "/op_meme");

        public    override void ProcessPhoto(string fileID) => DoPhoto(fileID, Memes.MakeMeme);
        public    override void ProcessStick(string fileID) => DoStick(fileID, Memes.MakeMemeFromSticker);
        protected override void ProcessVideo(string fileID) => DoVideo(fileID, Memes.MakeVideoMeme);

        protected override void ParseOptions()
        {
            /*MemeGenerator.UseCustomBg = Memes.Sticker && !empty && _custom_bg.IsMatch(dummy);
            if (MemeGenerator.UseCustomBg)
            {
                ParseColorOption(_custom_bg, ref dummy, ref MemeGenerator.CustomBg, ref MemeGenerator.UseCustomBg);
            }*/

            MemeGenerator.ExtraFonts.CheckKey(Request.Empty, ref Request.Dummy);
            MemeGenerator.FontMultiplier = !Request.Empty && _fontSS.IsMatch(Request.Dummy) ? GetInt(_fontSS, ref Request.Dummy) : 10;
            MemeGenerator.ShadowOpacity  = !Request.Empty && _shadow.IsMatch(Request.Dummy) ? Math.Clamp(GetInt(_shadow, ref Request.Dummy), 0, 100) : 100;
            MemeGenerator.WrapText       =  Request.Empty || !CheckMatch(ref Request.Dummy, _nowrap);
            MemeGenerator.ColorText      = !Request.Empty &&  CheckMatch(ref Request.Dummy, _colorText);
        }

        protected override DgText GetMemeText(string? text)
        {
            var add_bottom_text  = !Request.Empty && CheckMatch(ref Request.Dummy, _add_bottom);
            var only_bottom_text = !Request.Empty && CheckMatch(ref Request.Dummy, _only_bottom);
            var only_top_text    = !Request.Empty && CheckMatch(ref Request.Dummy, _top_only);
            var matchCaps        = !Request.Empty && CheckMatch(ref Request.Dummy, _caps);
            
            var gen = string.IsNullOrEmpty(text);
            var caps = matchCaps && (gen || _caps.IsMatch(Request.Command));

            string a, b;
            if (gen)
            {
                (a, b) = (Baka.Generate(), Baka.Generate());

                var c = Extension.Random.Next(10);
                if      (only_top_text)              b = "";
                else if (c == 0 || only_bottom_text) a = ""; // 1/10 >> bottom only
                else if (a.Length > 25) // upper text is too big
                {
                    if (c > 5) (a, b) = ("", a); // 4/10 >> bottom only
                    else b = "";                 // 5/10 >> top    only
                }
            }
            else
            {
                if (text!.Contains('\n'))
                {
                    var s = text.Split('\n', 2);
                    (a, b) = (s[0], s[1]);
                }
                else
                {
                    a = text;
                    b = add_bottom_text ? Baka.Generate() : "";
                }
            }
            return new DgText(AdjustCase(a), AdjustCase(b));

            string AdjustCase(string s) => caps ? s.ToLetterCase(LetterCaseMode.Upper) : s;
        }

        private int GetInt(Regex x, ref string dummy)
        {
            var match = x.Match(dummy);
            var value = int.Parse(match.Groups[1].Value);
            for (var i = match.Groups.Count - 1; i > 0; i--)
            {
                CutCaptureOut(match.Groups[i], ref dummy);
            }

            return value;
        }

        private static readonly Regex  _add_bottom = new(@"^\/meme\S*(s)\S*", RegexOptions.IgnoreCase);
        private static readonly Regex _only_bottom = new(@"^\/meme\S*(d)\S*", RegexOptions.IgnoreCase);
        private static readonly Regex    _top_only = new(@"^\/meme\S*(t)\S*", RegexOptions.IgnoreCase);
        private static readonly Regex        _caps = new(@"^\/meme\S*(u)\S*", RegexOptions.IgnoreCase);
        private static readonly Regex      _nowrap = new(@"^\/meme\S*(w)\S*", RegexOptions.IgnoreCase);
        private static readonly Regex   _colorText = new(@"^\/meme\S*(c)\S*", RegexOptions.IgnoreCase);
        private static readonly Regex   _custom_bg = new(@"^\/meme\S*#([A-Za-z]+)#\S*",  RegexOptions.IgnoreCase);
        private static readonly Regex      _fontSS = new(@"^\/meme\S*?(\d{1,3})("")\S*", RegexOptions.IgnoreCase);
        private static readonly Regex      _shadow = new(@"^\/meme\S*?(\d{1,3})(%)\S*",  RegexOptions.IgnoreCase);

        public static void ParseColorOption(Regex regex, ref string dummy, ref Color colorProperty, ref bool useColorProperty)
        {
            var c = regex.Match(dummy).Groups[1].Value;
            dummy = dummy.Replace(c, "");
            if (c == c.ToLower() || c == c.ToUpper()) c = c.ToLetterCase(LetterCaseMode.Sentence);
            var b = Enum.IsDefined(typeof(KnownColor), c);
            if (b)  colorProperty = Color.FromName(c);
            else useColorProperty = false;
        }
    }

    public interface ImageProcessor
    {
        ImageProcessor SetUp(int w, int h);

        void ProcessPhoto(string fileID);
        void ProcessStick(string fileID);
    }
}