using System.Text.RegularExpressions;

namespace Witlesss.Commands.Meme
{
    public class MemeDeepFryer : MakeMemeCore<int>, ImageProcessor
    {
        protected override Regex _cmd { get; } = new(@"^\/nuke(\S*) *", RegexOptions.IgnoreCase);

        protected override string Log_PHOTO(int x) => $"NUKED [{(x == 1 ? "*" : x)}]";
        protected override string Log_STICK(int x) => $"NUKED [{(x == 1 ? "*" : x)}] STICKER";

        protected override string Log_VIDEO { get; } = "NUKED [*] VID";
        protected override string VideoName { get; } = "nuke_fap_club.mp4";

        protected override string Options => Baka.Meme.OptionsN;
        protected override string Command { get; } = "/nuke";

        public ImageProcessor SetUp(int w, int h) // Needs more nuking!
        {
            JpegCoder.PassQuality(Baka);
            
            return this;
        }

        public override void Run() => Run("Ядерные отходы");

        public    override void ProcessPhoto(string fileID) => DoPhoto(fileID, Memes.DeepFryImage);
        public    override void ProcessStick(string fileID) => DoStick(fileID, Memes.DeepFryStick, false);
        protected override void ProcessVideo(string fileID) => DoVideo(fileID, Memes.DeepFryVideo);

        protected override int GetMemeText(string text) => 0;

        protected override bool CropVideoNotes { get; } = false;
    }
}