using PF_Bot.Core;
using PF_Bot.Core.Internet.Boards;
using PF_Bot.Handlers.Manage.Packs.Core;

namespace PF_Bot.Handlers.Manage.Packs;

public class EatPlanks : ChanEaterCore
{
    //      /planks
    //      /planks info  <┬─ SAME
    //      /plank  info  <┘
    //      /plank [a] [search query]
    //      /plank [thread/board]
    //      /plank Y-M-D a.N    <- [Y-M-D a.N.json]

    private readonly PlankService _chan = App.Chan2;

    private Uri? _uri;

    protected override ImageBoardContext Ctx => ImageBoardContext.Chan2;

    protected override string? GetSourceAnnotation()
    {
        return _uri is null ? null : string.Format(FUSE_SOURCE, _uri, _uri.LocalPath.Replace(".html", ""));
    }

    protected override async Task EatOnlineData(string url)
    {
        _uri = UrlOrBust(ref url);

        var board = _uri.Segments[1].Replace("/", "");

        if      (url.Contains("/res/")) await EatSingleThread(url, board);
        else                            await EatWholeBoard  (url, board);
    }

    private async Task EatSingleThread(string url, string board)
    {
        var name = $"{board}.{_uri!.Segments[3].Replace("/", "")}".Replace(".html", "");
        try
        {
            var replies = _chan.GetThreadDiscussion(url).ToList();
            await EatMany(replies, name);
        }
        catch
        {
            Bot.SendMessage(Origin, Bot.GetSillyErrorMessage());
        }
    }

    private async Task EatWholeBoard(string url, string board)
    {
        var name = board;

        var threads = _chan.GetSomeThreads(url);
        var tasks = threads.Select(x => _chan.GetThreadDiscussionAsync(x));

        await RespondAndStartEating(tasks, name);
    }

    protected override async Task EatOnlineFind(string[] args)
    {
        var name = string.Join('_', args);

        var threads = _chan.GetSearchResults(args[0], args[1]);
        var tasks = threads.Select(x => _chan.GetThreadDiscussionAsync(x));

        await RespondAndStartEating(tasks, name);
    }
}