using Telegram.Bot.Types;
using Witlesss.Commands.Packing.Core;
using Witlesss.Services.Internet.Boards;

namespace Witlesss.Commands.Packing;

public class EatBoards : ChanEaterCore
{
    //      /boards
    //      /boards info  <┬─ SAME
    //      /board  info  <┘
    //      /board [_/a] [search query]
    //      /board [thread/board/archive]
    //      /board Y-M-D a.N    <- [Y-M-D a.N.json]

    private static readonly BoardService _chan = new();
    private static readonly Lazy<List<BoardGroup>> _boards = new(_chan.GetBoardList(File_4chanHtmlPage));

    private string _name = default!;
    private Uri     _uri = default!;

    protected override string ArchivePath => Dir_Board;
    protected override string CommandString => "board";
    protected override string Manual   => BOARD_MANUAL;
    protected override string FileName => _name;
    protected override List<BoardGroup> Boards => _boards.Value;

    public new static void HandleCallback(CallbackQuery query, string[] data)
    {
        var pagination = query.GetPagination(data);

        if (data[0] == "b") SendBoardList(pagination, _boards.Value);
        else                SendSavedList(pagination, Dir_Board);
    }

    protected override string GetSourceAnnotation()
    {
        var shortURL = _uri.Segments[2].Contains("search") ? "desuarchive.org" : _uri.LocalPath;
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
        _name = $"{board}.{_uri.Segments[3].Replace("/", "")}";
        try
        {
            var replies = _chan.GetThreadDiscussion(url).ToList();
            await EatMany(replies, Size, Limit);
        }
        catch
        {
            Bot.SendMessage(Chat, Bot.GetSillyErrorMessage());
        }
    }

    private async Task EatWholeBoard(string url, string board)
    {
        _name = board;

        var threads = _chan.GetAllActiveThreads(url);
        var tasks = threads.Select(x => _chan.GetThreadDiscussionAsync(url + x));

        await RespondAndStartEating(tasks);
    }

    private async Task EatArchive(string url, string board)
    {
        _name = $"{board}.zip";

        var threads = _chan.GetAllArchivedThreads(url);
        var tasks = threads.Select(x => _chan.GetThreadDiscussionAsync("https://" + _uri.Host + x));

        await RespondAndStartEating(tasks);
    }

    protected override async Task EatOnlineFind(string[] args)
    {
        var url = _chan.GetDesuSearchURL(args[0], args[1]);

        _uri = new Uri(url);
        _name = string.Join('_', args);
            
        var threads = _chan.GetSearchResults(url);
        var tasks = threads.Select(x => _chan.GetThreadDiscussionAsync(x));

        await RespondAndStartEating(tasks);
    }
}