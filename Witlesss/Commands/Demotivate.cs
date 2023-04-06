using System.Text.RegularExpressions;
using Telegram.Bot.Types;

namespace Witlesss.Commands
{
    public class Demotivate : MakeMemeCore, ImageProcessor
    {
        public Demotivate() : base(new Regex(@"^\/d[vg]\S* *", RegexOptions.IgnoreCase)) { }

        private bool REPEAT_RX() => Text is { } && Regex.IsMatch(Text, @"^\/d[vg]\S*\d+\S*");
        private string D_PHOTO(int x) => $"DEMOTIVATOR [{(x == 1 ? "_" : x)}]";
        
        private readonly string D_VIDEO = "DEMOTIVATOR [^] VID";
        private readonly string D_STICK = "DEMOTIVATOR [#] STICKER";

        public ImageProcessor SetUp(int w, int h)
        {
            SelectModeAuto(w, h);
            JpegCoder.PassQuality(Baka);

            return this;
        }

        public override void Run() => Run(ProcessMessage, "Демотиваторы");

        private bool ProcessMessage(Message mess)
        {
            if (mess == null) return false;
            
            if      (mess.Photo != null)
                ProcessPhoto(mess.Photo[^1].FileId);
            else if (mess.Animation is { })
                ProcessVideo(mess.Animation.FileId);
            else if (mess.Sticker is { IsVideo: true })
                ProcessVideo(mess.Sticker.FileId);
            else if (mess.Video is { })
                ProcessVideo(mess.Video.FileId);
            else if (mess.VideoNote is { })
                ProcessVideo(mess.VideoNote.FileId);
            else if (mess.Sticker is { IsAnimated: false })
                ProcessStick(mess.Sticker.FileId);
            else return false;
            
            return true;
        }

        public  void ProcessPhoto(string fileID) => DoPhoto(fileID, D_PHOTO, M.MakeDemotivator, REPEAT_RX());
        public  void ProcessStick(string fileID) => DoStick(fileID, D_STICK, M.MakeStickerDemotivator);
        private void ProcessVideo(string fileID) => DoVideo(fileID, D_VIDEO, M.MakeVideoDemotivator);

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

        private void SelectModeAuto(float w, float h) => SetMode(w / h > 1.6 ? DgMode.Wide : DgMode.Square);
        private void SetMode(DgMode mode = DgMode.Square) => Bot.MemeService.Mode = mode;
    }
}