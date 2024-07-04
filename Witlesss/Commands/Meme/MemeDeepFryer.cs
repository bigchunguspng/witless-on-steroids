using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Witlesss.Backrooms.SerialQueue;
using Witlesss.MediaTools;
using Witlesss.Memes;

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
        protected override string Suffix  { get; } = "-Nuked";

        protected override string? DefaultOptions => Baka.Options?.Nuke;


        protected override Task Run() => RunInternal("Ядерные отходы");

        protected override void ParseOptions() { }

        protected override int GetMemeText(string? text) => 0; // Needs more nuking!

        protected override bool CropVideoNotes  { get; } = false;
        protected override bool ConvertStickers { get; } = false;

        // LOGIC

        private static readonly DukeNukem _nukem = new();
        private static readonly SerialTaskQueue _queue = new();

        protected override IMemeGenerator<int> MemeMaker => _nukem;
        protected override SerialTaskQueue Queue => _queue;
    }

    public class DukeNukem : IMemeGenerator<int>
    {
        public string GenerateMeme(MemeFileRequest request, int text)
        {
            return new F_Process(request.SourcePath)
                .DeepFry(request.GetQscale())
                .OutputAs(request.TargetPath).Result;
        }

        public Task<string> GenerateVideoMeme(MemeFileRequest request, int text)
        {
            var size = FFMpegXD.GetPictureSize(request.SourcePath).GrowSize().ValidMp4Size();

            return new F_Process(request.SourcePath)
                .DeepFryVideo(size.Ok(), request.GetCRF())
                .OutputAs(request.TargetPath);
        }
    }
}