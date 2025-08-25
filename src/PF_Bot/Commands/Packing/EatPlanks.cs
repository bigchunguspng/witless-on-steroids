using PF_Bot.Commands.Packing.Core;
using PF_Bot.Services.Internet.Boards;

namespace PF_Bot.Commands.Packing;

public class EatPlanks : ChanEaterCore
{
    //      /planks
    //      /planks info  <┬─ SAME
    //      /plank  info  <┘
    //      /plank [a] [search query]
    //      /plank [thread/board]
    //      /plank Y-M-D a.N    <- [Y-M-D a.N.json]

    private static readonly PlankService _chan = new();
    private static readonly Lazy<List<BoardGroup>> _boards = new(_chan.GetBoardList(File_2chanHtmlPage));

    private string _name = default!;
    private Uri? _uri;

    protected override string ArchivePath => Dir_Plank;
    protected override string CallbackKey => "p";
    protected override string CommandName => "plank";
    protected override string BoardsTitle => BOARDS_2CHAN;
    protected override string Manual      => PLANK_MANUAL;
    protected override string UnknownURL  => UNKNOWN_LINK_2CHAN;
    protected override string EmojiLogo   => "⚡️";
    protected override string FileName    => _name;
    protected override List<BoardGroup> Boards => _boards.Value;

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
        _name = $"{board}.{_uri!.Segments[3].Replace("/", "")}".Replace(".html", "");
        try
        {
            var replies = _chan.GetThreadDiscussion(url).ToList();
            await EatMany(replies, Size, Limit);
        }
        catch
        {
            Bot.SendMessage(Origin, Bot.GetSillyErrorMessage());
        }
    }

    private async Task EatWholeBoard(string url, string board)
    {
        _name = board;

        var threads = _chan.GetSomeThreads(url);
        var tasks = threads.Select(x => _chan.GetThreadDiscussionAsync(x));

        await RespondAndStartEating(tasks);
    }

    protected override async Task EatOnlineFind(string[] args)
    {
        _name = string.Join('_', args);

        var threads = _chan.GetSearchResults(args[0], args[1]);
        var tasks = threads.Select(x => _chan.GetThreadDiscussionAsync(x));

        await RespondAndStartEating(tasks);
    }
}