using System.Text.RegularExpressions;

namespace Witlesss.Commands
{
    public class MakeMeme : MakeMemeCore<DgText>, ImageProcessor
    {
        public MakeMeme() : base(new Regex(@"^\/meme\S* *", RegexOptions.IgnoreCase)) { }

        private static bool REPEAT_RX() => Text is { } && Regex.IsMatch(Text, @"^\/meme\S*(?<!ms)[2-9](?!\d?%)\S*");
        private static string M_PHOTO(int x) => $"MEME [{(x == 1 ? "M" : x)}]";

        private const string M_VIDEO = "MEME [M] VID";
        private const string M_STICK = "MEME [M] STICKER";

        public ImageProcessor SetUp(int w, int h)
        {
            JpegCoder.PassQuality(Baka);

            return this;
        }

        public override void Run() => Run("Мемы");

        public    override void ProcessPhoto(string fileID) => DoPhoto(fileID, M_PHOTO, M.MakeMeme, REPEAT_RX());
        public    override void ProcessStick(string fileID) => DoStick(fileID, M_STICK, M.MakeMemeFromSticker);
        protected override void ProcessVideo(string fileID) => DoVideo(fileID, M_VIDEO, M.MakeVideoMeme);

        protected override DgText GetMemeText(string text)
        {
            string a, b;
            if (string.IsNullOrEmpty(text))
            {
                (a, b) = (Baka.Generate(), Baka.Generate());

                var c = Random.Next(10);
                if (c == 0 || OnlyBottomText) a = "";
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
                    b = AddBottomText ? Baka.Generate() : "";
                }
            }
            return new DgText(a, b);
        }

        private static bool  AddBottomText => Text != null && Text.Split()[0].Contains('s');
        private static bool OnlyBottomText => Text != null && Text.Split()[0].Contains('d');

        public static ColorMode Dye => Baka.Meme.Dye;
    }

    public interface ImageProcessor
    {
        ImageProcessor SetUp(int w, int h);

        void ProcessPhoto(string fileID);
        void ProcessStick(string fileID);
    }
}