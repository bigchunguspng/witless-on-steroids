using PF_Bot.Backrooms.Helpers;
using PF_Bot.Backrooms.Listing;
using PF_Bot.Core.Memes.Generators;
using PF_Bot.Core.Memes.Options;
using PF_Bot.Core.Memes.Shared;
using PF_Bot.Routing_New.Routers;

namespace PF_Bot.Handlers.Memes
{
    public class Nuke : MakeMemeCore<int>
    {
        private MemeOptions_Nuke _options;

        protected override IMemeGenerator<int> MemeMaker => new DukeNukem(_options, Chat);

        protected override Regex _rgx_cmd { get; } = new(@"^\/nuke(\S*)", RegexOptions.Compiled);

        protected override string VideoName => "piece_fap_bot-nuke.mp4";

        protected override string Log_STR => "NUKED";
        protected override string Log_CMD => "/nuke";
        protected override string Suffix  => "Nuked"; // Needs more nuking!

        protected override string? DefaultOptions => Data.Options?.Nuke;


        protected override Task Run()
        {
            if /**/ (Args is "log" or "logs" || Context.Command!.StartsWith("/nuke_log"))
                ListingNukes.SendNukeLog(new ListPagination(Origin, PerPage: 5));
            else if (Args is null)
                return RunInternal("nuke\n⏳История фильтров: /nuke_log");

            return Task.CompletedTask;
        }

        protected override bool CropVideoNotes   => false;
        protected override bool ResultsAreRandom => true;

        protected override void ParseOptions()
        {
            _options.Depth = Options.GetInt(_r_depth, 1);
        }

        protected override int GetMemeText(string? text) => 0;

        private static readonly Regex
            _r_depth = new(@"([1-9])("")", RegexOptions.Compiled);
    }

    public class Nuke_Callback : CallbackHandler
    {
        protected override Task Run()
        {
            var pagination = Query.GetPagination(Content);

            ListingNukes.SendNukeLog(pagination);
            return Task.CompletedTask;
        }
    }
}