using PF_Bot.Features_Aux.Listing;
using PF_Bot.Features_Main.Memes.Core.Generators;
using PF_Bot.Features_Main.Memes.Core.Options;
using PF_Bot.Features_Main.Memes.Core.Shared;
using PF_Bot.Routing.Callbacks;

namespace PF_Bot.Features_Main.Memes.Commands;

public class Nuke : Meme_Core<int>
{
    private MemeOptions_Nuke _options;

    protected override IMemeGenerator<int> MemeMaker => new DukeNukem(_options, Chat);

    protected override string? DefaultOptions => Data.Options?.Nuke;

    protected override MemeMakerContext Ctx => MemeMakerContext.Nuke;


    protected override Task Run()
    {
        if /**/ (Args is "log" or "logs" || Options.StartsWith("_log"))
            ListingNukes.SendNukeLog(new ListPagination(Origin, PerPage: 5));
        else
            return RunInternal("nuke\n⏳История фильтров: /nuke_log");

        return Task.CompletedTask;
    }

    protected override bool CropVideoNotes   => false;
    protected override bool ResultsAreRandom => true;

    protected override void ParseOptions()
    {
        _options.Depth = MemeOptions.GetInt(_r_depth, 1);
    }

    protected override int GetMemeText(string? text) => 0;

    private static readonly Regex
        _r_depth = new(@"([1-9])("")", RegexOptions.Compiled); // Needs more nuking!
}

public class Nuke_Callback : CallbackHandler
{
    protected override Task Run()
    {
        ListingNukes.SendNukeLog(GetPagination(Content));
        return Task.CompletedTask;
    }
}