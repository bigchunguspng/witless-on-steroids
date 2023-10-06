using System.Text.RegularExpressions;
using static Witlesss.XD.DgMode;

namespace Witlesss.Commands
{
    public class Demotivate : MakeMemeCore<DgText>, ImageProcessor
    {
        protected override Regex _cmd { get; } = new(@"^\/d[vg](\S*) *", RegexOptions.IgnoreCase);

        protected override string Log_PHOTO(int x) => $"DEMOTIVATOR [{(x == 1 ? "_" : x)}]";

        protected override string Log_VIDEO { get; } = "DEMOTIVATOR [^] VID";
        protected override string Log_STICK { get; } = "DEMOTIVATOR [#] STICKER";
        protected override string VideoName => $"piece_fap_club-d{(Memes.Mode == Square ? "g" : "v")}.mp4";

        public ImageProcessor SetUp(int w, int h)
        {
            JpegCoder.PassQuality(Baka);
            SelectModeAuto(w, h);
            return this;
        }

        public override void Run() => Run("Демотиваторы💀");

        public    override void ProcessPhoto(string fileID) => DoPhoto(fileID, Memes.MakeDemotivator);
        public    override void ProcessStick(string fileID) => DoStick(fileID, Memes.MakeStickerDemotivator);
        protected override void ProcessVideo(string fileID) => DoVideo(fileID, Memes.MakeVideoDemotivator);

        protected override DgText GetMemeText(string text)
        {
            var empty = Text is null;
            var input = empty ? "" : Text.Replace(Config.BOT_USERNAME, "");

            DemotivatorDrawer.AddLogo = empty || !_no_logo.IsMatch(input);
            
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

        private static readonly Regex _no_logo = new(@"^\/d[vg]\S*n\S* *", RegexOptions.IgnoreCase);

        public Demotivate SetUp(DgMode mode)
        {
            SetMode(mode);

            return this;
        }

        private static void SelectModeAuto(float w, float h) => SetMode(w / h > 1.6 ? Wide : Square);
        private static void SetMode(DgMode mode = Square) => Memes.Mode = mode;
    }
}