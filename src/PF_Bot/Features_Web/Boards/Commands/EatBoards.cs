using PF_Bot.Core;
using PF_Bot.Features_Web.Boards.Core;

namespace PF_Bot.Features_Web.Boards.Commands;

public class EatBoards : EatBoard_Core
{
    //      /boards
    //      /boards info  <┬─ SAME
    //      /board  info  <┘
    //      /board [_/a](!) [search query]
    //      /board [thread/board/archive]
    //      /board Y-M-D a.N    <- [Y-M-D a.N.json]

    private readonly BoardService _chan = App.Chan4;

    private Uri _uri = null!;

    protected override ImageBoardContext Ctx => ImageBoardContext.Chan4;

    protected override string GetSourceAnnotation()
    {
        var shortURL = _uri.Segments.Length > 2 && _uri.Segments[2].Contains("search")
            ? "desuarchive.org"
            : _uri.LocalPath;
        return string.Format(FUSE_SOURCE, _uri, shortURL);
    }

    protected override async Task EatOnlineData(string url)
    {
        _uri = UrlOrBust(ref url);

        var board = _uri.Segments[1].Replace("/", "");

        if      (url.Contains("/thread/")) await EatSingleThread(url, board);
        else if (url.EndsWith("/archive")) await EatArchive     (url, board); 
        else                               await EatWholeBoard  (url, board);
    }

    private async Task EatSingleThread(string url, string board)
    {
        var name = $"{board}.{_uri.Segments[3].Replace("/", "")}";
        try
        {
            var replies = await _chan.GetThreadDiscussionAsync(url);
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

        var threads = _chan.GetAllActiveThreads(url);
        var tasks = threads.Select(x => _chan.GetThreadDiscussionAsync(url + x));

        await RespondAndStartEating(tasks, name);
    }

    private async Task EatArchive(string url, string board)
    {
        var name = $"{board}.zip";

        var threads = _chan.GetAllArchivedThreads(url);
        var tasks = threads.Select(x => _chan.GetThreadDiscussionAsync("https://" + _uri.Host + x));

        await RespondAndStartEating(tasks, name);
    }

    protected override async Task EatOnlineFind(string[] args)
    {
        var bySubject = args[0].Contains('!');
        var url = bySubject
            ? _chan.GetDesuSearchURLSubject(args[0].Replace("!", ""), args[1])
            : _chan.GetDesuSearchURLText   (args[0],                  args[1]);

        _uri = new Uri(url);
        var name = string.Join('_', args);

        var threads = _chan.GetSearchResults(url);
        var tasks = threads.Select(x => _chan.GetThreadDiscussionAsync(x));

        await RespondAndStartEating(tasks, name);
    }
}