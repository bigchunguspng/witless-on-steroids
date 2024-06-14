using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Witlesss.Backrooms.SerialQueue;
using Witlesss.MediaTools;
using static Witlesss.XD.DgMode;

namespace Witlesss.Commands.Meme
{
    public class Demotivate : MakeMemeCore<DgText>, ImageProcessor
    {
        protected override Regex _cmd { get; } = new(@"^\/d[vg](\S*) *", RegexOptions.IgnoreCase);

        protected override string Log_PHOTO(int x) => $"DEMOTIVATOR [{(x == 1 ? "_" : x)}]";
        protected override string Log_STICK(int x) => $"DEMOTIVATOR [{(x == 1 ? "#" : x)}] STICKER";

        protected override string Log_VIDEO { get; } = "DEMOTIVATOR [^] VID";
        protected override string VideoName => $"piece_fap_club-d{(_mode == Square ? "g" : "v")}.mp4";

        protected override string Command => _mode == Square ? "/dg" : "/dv";

        protected override string? DefaultOptions => Baka.Meme.OptionsG;

        public ImageProcessor SetUp(int w, int h)
        {
            ImageSaver.PassQuality(Baka);
            SelectModeAuto(w, h);
            return this;
        }

        protected override Task Run() => RunInternal("Демотиваторы💀");

        protected override void ParseOptions()
        {
            DemotivatorDrawer.AddLogo = !OptionsParsing.Check(Request, _no_logo);
        }

        protected override DgText GetMemeText(string? text)
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

        private static readonly Regex _no_logo = new(@"^\/d[vg]\S*n\S* *", RegexOptions.IgnoreCase);

        public Demotivate SetUp(DgMode mode)
        {
            _mode = mode;
            return this;
        }

        private static void SelectModeAuto(float w, float h) => _mode = w / h > 1.6 ? Wide : Square;

        // LOGIC

        private static readonly DemotivatorDrawer[] _drawers = [new DemotivatorDrawer(), new DemotivatorDrawer(1280)];
        private static readonly SerialTaskQueue _queue = new();
        private static DemotivatorDrawer Drawer => _drawers[(int) _mode];
        private static DgMode _mode;

        protected override Task<string> MakeMemeImage(string path, DgText text)
        {
            return _queue.Enqueue(() => Drawer.MakeDemotivator(path, text));
        }

        protected override Task<string> MakeMemeStick(string path, DgText text, string extension)
        {
            //return MakeDemotivator(Convert(path, extension), text);
            return MakeMemeImage(path, text);
        }

        protected override Task<string> MakeMemeVideo(string path, DgText text)
        {
            return _queue.Enqueue
            (
                () => new F_Combine(path, Drawer.MakeFrame(text)).Demo(Memes.Quality, Drawer).Output("-D")
            );
        }
    }
}