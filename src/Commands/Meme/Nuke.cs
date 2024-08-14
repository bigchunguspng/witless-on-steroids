using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Witlesss.Backrooms.Helpers;
using Witlesss.Backrooms.SerialQueue;
using Witlesss.Commands.Meme.Core;
using Witlesss.MediaTools;
using Witlesss.Memes.Shared;

namespace Witlesss.Commands.Meme
{
    public class Nuke : MakeMemeCore<int>, ImageProcessor
    {
        protected override Regex _cmd { get; } = new(@"^\/nuke(\S*) *", RegexOptions.IgnoreCase);

        protected override string Log_PHOTO(int x) => $"NUKED [{(x == 1 ? "*" : x)}]";
        protected override string Log_STICK(int x) => $"NUKED [{(x == 1 ? "*" : x)}] STICKER";

        protected override string Log_VIDEO => "NUKED [*] VID";
        protected override string VideoName => "piece_fap_club-nuke.mp4";

        protected override string Command => "/nuke";
        protected override string Suffix  => "-Nuked"; // Needs more nuking!

        protected override string? DefaultOptions => Baka.Options?.Nuke;


        protected override Task Run() => RunInternal("ядерные отходы", "nuke");

        protected override bool ResultsAreRandom => true;

        protected override void ParseOptions()
        {
            DukeNukem.Depth = OptionsParsing.GetInt(Request, _depth, 1);
        }

        protected override int GetMemeText(string? text) => 0;

        private static readonly Regex _depth = new(@"^\/nuke\S*?([1-9])("")\S*");

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

            for (var i = 0; i < Depth.Clamp(3); i++)
            {
                path = await new F_Process(path)
                    .DeepFryVideo(size.Ok(), request.GetCRF())
                    .OutputAs(UniquePath(request.TargetPath));
            }

            return path;
        }
    }
}