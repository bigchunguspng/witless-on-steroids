using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using static Witlesss.Memes;

namespace Witlesss.Commands
{
    public class MakeMeme : MakeMemeCore, ImageProcessor
    {
        public MakeMeme() : base(new Regex(@"^\/meme\S* *", RegexOptions.IgnoreCase)) { }

        private bool REPEAT_RX() => Text is { } && Regex.IsMatch(Text, @"^\/meme\S*\d+\S*");
        private string M_PHOTO(int x) => $"MEME [{(x == 1 ? "M" : x)}]";

        private readonly string M_VIDEO = "MEME [M] VID";
        private readonly string M_STICK = "MEME [M] STICKER";

        public ImageProcessor SetUp(int w, int h)
        {
            JpegCoder.PassQuality(Baka);

            return this;
        }

        public override void Run() => Run(ProcessMessage, "Мемы");

        private bool ProcessMessage(Message mess)
        {
            if (mess == null) return false;

            if (mess.Photo is { } p)
            {
                ProcessPhoto(p[^1].FileId);
            }
            else if (mess.Animation is { } a)
            {
                PassSize(a);
                ProcessVideo(a.FileId);
            }
            else if (mess.Sticker is { IsVideo: true } s)
            {
                PassSize(s);
                ProcessVideo(s.FileId);
            }
            else if (mess.Video is { } v)
            {
                PassSize(v);
                ProcessVideo(v.FileId);
            }
            else if (mess.VideoNote is { } n)
            {
                PassSize(272);
                ProcessVideo(n.FileId);
            }
            else if (mess.Sticker is { IsAnimated: false} ss)
            {
                ProcessStick(ss.FileId);
            }
            else return false;

            return true;
        }

        public  void ProcessPhoto(string fileID) => DoPhoto(fileID, M_PHOTO, M.MakeMeme, REPEAT_RX());
        public  void ProcessStick(string fileID) => DoStick(fileID, M_STICK, M.MakeMemeFromSticker);
        private void ProcessVideo(string fileID) => DoVideo(fileID, M_VIDEO, M.MakeVideoMeme);

        protected override DgText GetMemeText(string text)
        {
            string a, b;
            if (string.IsNullOrEmpty(text))
            {
                (a, b) = (Baka.Generate(), Baka.Generate());

                var c = Random.Next(10);
                if (c == 0) a = "";
                if (a.Length > 25)
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
                    b = AddBottomText() ? Baka.Generate() : "";
                }
            }
            return new DgText(a, b);
        }

        private bool AddBottomText() => Text != null && Text.Split()[0].Contains('s');

        public static ColorMode Dye  => Baka.Meme.Dye;
    }

    public interface ImageProcessor
    {
        ImageProcessor SetUp(int w, int h);

        void ProcessPhoto(string fileID);
        void ProcessStick(string fileID);
    }
}