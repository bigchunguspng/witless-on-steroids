using System;
using System.Drawing;
using System.Text.RegularExpressions;

namespace Witlesss.Commands.Meme
{
    public class MakeMeme : MakeMemeCore<DgText>, ImageProcessor
    {
        protected override Regex _cmd { get; } = new(@"^\/meme(\S*) *", RegexOptions.IgnoreCase);

        protected override string Log_PHOTO(int x) => $"MEME [{(x == 1 ? "M" : x)}]";
        protected override string Log_STICK(int x) => $"MEME [{(x == 1 ? "M" : x)}] STICKER";

        protected override string Log_VIDEO { get; } = "MEME [M] VID";
        protected override string VideoName { get; } = "piece_fap_club-meme.mp4";
        
        protected override string Options => Baka.Meme.OptionsM;
        protected override string Command { get; } = "/meme";

        public ImageProcessor SetUp(int w, int h)
        {
            JpegCoder.PassQuality(Baka);

            return this;
        }

        public override void Run() => Run("Мемы", MEME_OPTIONS);

        public    override void ProcessPhoto(string fileID) => DoPhoto(fileID, Memes.MakeMeme);
        public    override void ProcessStick(string fileID) => DoStick(fileID, Memes.MakeMemeFromSticker);
        protected override void ProcessVideo(string fileID) => DoVideo(fileID, Memes.MakeVideoMeme);

        protected override DgText GetMemeText(string text)
        {
            var dummy = GetDummy(out var empty, out var command);

            MemeGenerator.UseCustomBack = Memes.Sticker && !empty && _custom_bg.IsMatch(dummy);
            if (MemeGenerator.UseCustomBack)
            {
                var c = _custom_bg.Match(dummy).Groups[1].Value;
                dummy = dummy.Replace(c, "");
                if (c == c.ToLower() || c == c.ToUpper()) c = c.ToLetterCase(LetterCaseMode.Sentence);
                var d = Enum.IsDefined(typeof(KnownColor), c);
                if (d) MemeGenerator.CustomBackground = Color.FromName(c);
                else   MemeGenerator.UseCustomBack = false;
            }

            MemeGenerator.ExtraFonts.CheckKey(empty, dummy);
            MemeGenerator.WrapText      =  empty ||     !_nowrap.IsMatch(dummy);
            MemeGenerator.UseItalic     = !empty &&      _italic.IsMatch(dummy);
            MemeGenerator.ColorText     = !empty &&   _colorText.IsMatch(dummy);
            var add_bottom_text         = !empty &&  _add_bottom.IsMatch(dummy);
            var only_bottom_text        = !empty && _only_bottom.IsMatch(dummy);
            var only_top_text           = !empty &&    _top_only.IsMatch(dummy);
            var matchCaps               = !empty &&        _caps.IsMatch(dummy);

            var gen = string.IsNullOrEmpty(text);
            var caps = matchCaps && (gen || _caps.IsMatch(command));

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
                if (text.Contains('\n'))
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

        private static readonly Regex      _nowrap = new(@"^\/meme\S*w\S*",  RegexOptions.IgnoreCase);
        private static readonly Regex  _add_bottom = new(@"^\/meme\S*s\S*",  RegexOptions.IgnoreCase);
        private static readonly Regex _only_bottom = new(@"^\/meme\S*d\S*",  RegexOptions.IgnoreCase);
        private static readonly Regex    _top_only = new(@"^\/meme\S*to\S*", RegexOptions.IgnoreCase);
        private static readonly Regex   _colorText = new(@"^\/meme\S*co\S*", RegexOptions.IgnoreCase);
        private static readonly Regex      _impact = new(@"^\/meme\S*im\S*", RegexOptions.IgnoreCase);
        private static readonly Regex      _italic = new(@"^\/meme\S*i\S*",  RegexOptions.IgnoreCase);
        private static readonly Regex        _caps = new(@"^\/meme\S*up\S*", RegexOptions.IgnoreCase);
        private static readonly Regex   _custom_bg = new(@"^\/meme\S*#([A-Za-z]+)#\S*", RegexOptions.IgnoreCase);
    }

    public interface ImageProcessor
    {
        ImageProcessor SetUp(int w, int h);

        void ProcessPhoto(string fileID);
        void ProcessStick(string fileID);
    }
}