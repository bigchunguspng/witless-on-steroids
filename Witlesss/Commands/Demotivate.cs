using System.Text.RegularExpressions;
using static Witlesss.XD.DgMode;

namespace Witlesss.Commands
{
    public class Demotivate : MakeMemeCore<DgText>, ImageProcessor
    {
        public Demotivate() : base(new Regex(@"^\/d[vg]\S* *", RegexOptions.IgnoreCase)) { }

        private static bool REPEAT_RX() => Text is not null && Regex.IsMatch(Text, @"^\/d[vg]\S*[1-9](?!\d?%)\S*");
        private static string D_PHOTO(int x) => $"DEMOTIVATOR [{(x == 1 ? "_" : x)}]";

        private const string D_VIDEO = "DEMOTIVATOR [^] VID";
        private const string D_STICK = "DEMOTIVATOR [#] STICKER";

        public ImageProcessor SetUp(int w, int h)
        {
            SelectModeAuto(w, h);
            JpegCoder.PassQuality(Baka);

            return this;
        }

        public override void Run() => Run("Демотиваторы");

        public    override void ProcessPhoto(string fileID) => DoPhoto(fileID, D_PHOTO, M.MakeDemotivator, REPEAT_RX());
        public    override void ProcessStick(string fileID) => DoStick(fileID, D_STICK, M.MakeStickerDemotivator);
        protected override void ProcessVideo(string fileID) => DoVideo(fileID, D_VIDEO, M.MakeVideoDemotivator);

        protected override DgText GetMemeText(string text)
        {
            string a, b = Baka.Generate();
            if (b.Length > 1) b = b[0] + b[1..].ToLower(); // lower text can't be UPPERCASE
            if (string.IsNullOrEmpty(text)) a = Baka.Generate();
            else
            {
                var s = text.Split('\n', 2);
                a = s[0];
                if (s.Length > 1) b = s[1];
            }
            return new DgText(a, b);
        }

        public Demotivate SetUp(DgMode mode)
        {
            SetMode(mode);

            return this;
        }

        private static void SelectModeAuto(float w, float h) => SetMode(w / h > 1.6 ? Wide : Square);
        private static void SetMode(DgMode mode = Square) => Bot.MemeService.Mode = mode;
    }
}