using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Witlesss.Backrooms.SerialQueue;
using Witlesss.MediaTools;

namespace Witlesss.Commands.Meme
{
    public class MemeDeepFryer : MakeMemeCore<int>, ImageProcessor
    {
        protected override Regex _cmd { get; } = new(@"^\/nuke(\S*) *", RegexOptions.IgnoreCase);

        protected override string Log_PHOTO(int x) => $"NUKED [{(x == 1 ? "*" : x)}]";
        protected override string Log_STICK(int x) => $"NUKED [{(x == 1 ? "*" : x)}] STICKER";

        protected override string Log_VIDEO { get; } = "NUKED [*] VID";
        protected override string VideoName { get; } = "nuke_fap_club.mp4";

        protected override string Command { get; } = "/nuke";

        protected override string? DefaultOptions => Baka.Meme.Options?.Nuke;

        public ImageProcessor SetUp(int w, int h) // Needs more nuking!
        {
            ImageSaver.PassQuality(Baka);

            return this;
        }

        protected override Task Run() => RunInternal("Ядерные отходы");

        protected override void ParseOptions() { }

        protected override int GetMemeText(string? text) => 0;

        protected override bool CropVideoNotes  { get; } = false;
        protected override bool ConvertStickers { get; } = false;

        // LOGIC

        private static readonly SerialTaskQueue _nukeQueue = new();

        protected override async Task<string> MakeMemeImage(string path, int text)
        {
            var extension = Memes.Sticker ? ".webp" : Path.GetExtension(path);
            return await new F_Process(path).DeepFry(Memes.Qscale).Output("-Nuked", extension);
        }

        protected override Task<string> MakeMemeStick(string path, int text, string extension)
        {
            return MakeMemeImage(path, text);
        }

        protected override async Task<string> MakeMemeVideo(string path, int text)
        {
            var size = SizeHelpers.GetImageSize_FFmpeg(path).GrowSize().ValidMp4Size();
            return await new F_Process(path).DeepFryVideo(size.Ok(), Memes.Quality).Output_WEBM_safe("-Nuked");
        }
    }
}