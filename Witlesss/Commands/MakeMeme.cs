using System;
using System.Drawing;
using System.Text.RegularExpressions;

namespace Witlesss.Commands
{
    public class MakeMeme : MakeMemeCore<DgText>, ImageProcessor
    {
        public MakeMeme() : base(new Regex(@"^\/meme(\S*) *", RegexOptions.IgnoreCase)) { }

        protected override string Log_PHOTO(int x) => $"MEME [{(x == 1 ? "M" : x)}]";

        protected override string Log_VIDEO { get; } = "MEME [M] VID";
        protected override string Log_STICK { get; } = "MEME [M] STICKER";
        protected override string VideoName { get; } = "piece_fap_club-meme.mp4";

        public ImageProcessor SetUp(int w, int h)
        {
            JpegCoder.PassQuality(Baka);

            return this;
        }

        public override void Run() => Run("Мемы");

        public    override void ProcessPhoto(string fileID) => DoPhoto(fileID, Memes.MakeMeme);
        public    override void ProcessStick(string fileID) => DoStick(fileID, Memes.MakeMemeFromSticker);
        protected override void ProcessVideo(string fileID) => DoVideo(fileID, Memes.MakeVideoMeme);

        protected override DgText GetMemeText(string text)
        {
            var empty = string.IsNullOrEmpty(Text);
            var dummy = empty ? "" : Text.Replace(Config.BOT_USERNAME, "");

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
            MemeGenerator.WrapText      =  empty ||     !_nowrap.IsMatch(dummy);
            var add_bottom_text         = !empty &&  _add_bottom.IsMatch(dummy);
            var only_bottom_text        = !empty && _only_bottom.IsMatch(dummy);

            string a, b;
            if (string.IsNullOrEmpty(text))
            {
                (a, b) = (Baka.Generate(), Baka.Generate());

                var c = Extension.Random.Next(10);
                if (c == 0 || only_bottom_text) a = "";
                else if (a.Length > 25)
                {
                    if (c > 5) (a, b) = ("", a);
                    else b = "";
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
            return new DgText(a, b);
        }

        private static readonly Regex      _nowrap = new(@"^\/meme\S*w\S* *", RegexOptions.IgnoreCase);
        private static readonly Regex  _add_bottom = new(@"^\/meme\S*s\S* *", RegexOptions.IgnoreCase);
        private static readonly Regex _only_bottom = new(@"^\/meme\S*d\S* *", RegexOptions.IgnoreCase);
        private static readonly Regex   _custom_bg = new(@"^\/meme\S*#([A-Za-z]+)#\S* *", RegexOptions.IgnoreCase);

        public static ColorMode Dye => Baka.Meme.Dye;
    }

    public interface ImageProcessor
    {
        ImageProcessor SetUp(int w, int h);

        void ProcessPhoto(string fileID);
        void ProcessStick(string fileID);
    }
}