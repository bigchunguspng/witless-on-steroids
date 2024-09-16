using Witlesss.Commands.Meme.Core;
using Witlesss.Memes;
using Witlesss.Memes.Shared;

namespace Witlesss.Commands.Meme
{
    public class Nuke : MakeMemeCore<int>
    {
        private static readonly DukeNukem _nukem = new();

        protected override IMemeGenerator<int> MemeMaker => _nukem;

        protected override Regex _cmd { get; } = new(@"^\/nuke(\S*)");

        protected override string VideoName => "piece_fap_bot-nuke.mp4";

        protected override string Log_STR => "NUKED";
        protected override string Command => "/nuke";
        protected override string Suffix  => "-Nuked"; // Needs more nuking!

        protected override string? DefaultOptions => Baka.Options?.Nuke;


        protected override Task Run() => RunInternal("nuke");

        protected override bool CropVideoNotes   => false;
        protected override bool ResultsAreRandom => true;

        protected override void ParseOptions()
        {
            DukeNukem.Depth = OptionsParsing.GetInt(Request, _depth, 1);
        }

        protected override int GetMemeText(string? text) => 0;

        private static readonly Regex _depth = new(@"^\/nuke\S*?([1-9])("")\S*");
    }
}