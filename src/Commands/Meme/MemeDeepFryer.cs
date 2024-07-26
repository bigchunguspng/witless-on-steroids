using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Witlesss.Backrooms.Helpers;
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
        protected override string Suffix  { get; } = "-Nuked"; // Needs more nuking!

        protected override string? DefaultOptions => Baka.Options?.Nuke;


        protected override Task Run() => RunInternal("Ядерные отходы");

        protected override bool ResultsAreRandom => true;

        protected override void ParseOptions()
        {
            DukeNukem.Depth = OptionsParsing.GetInt(Request, _depth, 1).Clamp(1, 9);
        }

        protected override int GetMemeText(string? text) => 0;

        private static readonly Regex _depth = new(@"^\/nuke\S*?(\d{1,3})("")\S*");

        protected override bool CropVideoNotes  => false;
        protected override bool ConvertStickers => false;

        // LOGIC

        private static readonly DukeNukem _nukem = new();
        private static readonly SerialTaskQueue _queue = new();

        protected override IMemeGenerator<int> MemeMaker => _nukem;
        protected override SerialTaskQueue Queue => _queue;
    }

    public class DukeNukem : IMemeGenerator<int>
    {
        public static int Depth = 1;

        public string GenerateMeme(MemeFileRequest request, int text)
        {
            var path = request.SourcePath;

            for (var i = 0; i < Depth; i++)
            {
                path = new F_Process(path)
                    .DeepFry(request.GetQscale())
                    .OutputAs(UniquePath(request.TargetPath)).Result;
            }

            return path;
        }

        public async Task<string> GenerateVideoMeme(MemeFileRequest request, int text)
        {
            var size = FFMpegXD.GetPictureSize(request.SourcePath).GrowSize().ValidMp4Size();

            var path = request.SourcePath;

            for (var i = 0; i < Depth; i++)
            {
                path = await new F_Process(path)
                    .DeepFryVideo(size.Ok(), request.GetCRF())
                    .OutputAs(UniquePath(request.TargetPath));
            }

            return path;
        }
    }
}