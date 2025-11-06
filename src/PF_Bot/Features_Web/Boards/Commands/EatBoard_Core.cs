using PF_Bot.Core;
using PF_Bot.Features_Aux.Listing;
using PF_Bot.Features_Aux.Packs.Commands;
using PF_Bot.Features_Web.Boards.Core;
using PF_Bot.Routing.Callbacks;

namespace PF_Bot.Features_Web.Boards.Commands;

public class ImageBoardContext
{
    public required FilePath ArchivePath { get; init; }

    public required string CallbackKey { get; init; }
    public required string CommandName { get; init; }
    public required string BoardsTitle { get; init; }
    public required string Manual      { get; init; }
    public required string UnknownURL  { get; init; }
    public required string EmojiLogo   { get; init; }

    public required Lazy<List<BoardGroup>> Boards_Lazy { get; init; }
    public               List<BoardGroup>  Boards => Boards_Lazy.Value;

    public static readonly ImageBoardContext
        Chan2 = new()
        {
            ArchivePath = Dir_Plank,
            CallbackKey = Registry.CallbackKey_Planks,
            CommandName = "plank",
            BoardsTitle = BOARDS_2CHAN,
            Manual      = PLANK_MANUAL,
            UnknownURL  = UNKNOWN_LINK_2CHAN,
            EmojiLogo   = "⚡️",
            Boards_Lazy = new Lazy<List<BoardGroup>>(App.Chan2.GetBoardList(File_2chanHtmlPage)),
        },
        Chan4 = new()
        {
            ArchivePath = Dir_Board,
            CallbackKey = Registry.CallbackKey_Boards,
            CommandName = "board",
            BoardsTitle = BOARDS_4CHAN,
            Manual      = BOARD_MANUAL,
            UnknownURL  = UNKNOWN_LINK_4CHAN,
            EmojiLogo   = "🍀",
            Boards_Lazy = new Lazy<List<BoardGroup>>(App.Chan4.GetBoardList(File_4chanHtmlPage)),
        };
}

public class ChanEaterCore_Callback(ImageBoardContext ctx) : CallbackHandler
{
    protected override Task Run()
    {
        var pagination = GetPagination(Content);

        if (Key == ctx.CallbackKey) ListingBoards.SendBoardList(ctx, pagination, ctx.Boards);
        else                        ListingBoards.SendSavedList(ctx, pagination);
        return Task.CompletedTask;
    }
}

public abstract class EatBoard_Core : Fuse
{
    protected abstract ImageBoardContext Ctx { get; }

    private FilePath    ArchivePath => Ctx.ArchivePath;
    private string      CommandName => Ctx.CommandName;
    private string      Manual      => Ctx.Manual;
    private string      UnknownURL  => Ctx.UnknownURL;
    private List<BoardGroup> Boards => Ctx.Boards;

    protected abstract string? GetSourceAnnotation();

    protected override async Task RunAuthorized()
    {
        if (Args is null)
        {
            if (Options.StartsWith('s'))
                ListingBoards.SendBoardList(Ctx, new ListPagination(Origin, PerPage: 2), Boards);
            else
                SendManual(Manual);
        }
        else
        {
            if      (Args.EndsWith("info"))
                ListingBoards.SendSavedList(Ctx, new ListPagination(Origin, PerPage: 10));
            else if (Args.CanBeSplitN())
            {
                var args = Args.SplitN(2);
                var filename = args[0].Contains('-');
                if (filename) await EatJsonFile();
                else          await EatOnlineFind(args);
            }
            else
                await EatOnlineData(url: Args);
        }
    }

    private async Task EatJsonFile()
    {
        var files = ArchivePath.GetFiles($"{Args}.json");
        if (files.Length == 0)
        {
            SetBadStatus();
            Bot.SendMessage(Origin, FUSE_FAIL_BOARD.Format($"{CommandName}s"));
        }
        else
            await EatFromJsonFile(files[0]);
    }

    protected abstract Task EatOnlineData(string url);
    protected abstract Task EatOnlineFind(string[] args);

    protected async Task RespondAndStartEating(IEnumerable<Task<List<string>>> tasks, string name)
    {
        var message = Bot.PingChat(Origin, BOARD_START);

        var threads = await Task.WhenAll(tasks);

        var text = BOARD_START_EDIT.Format(threads.Length);
        if (threads.Length > 150) text += MAY_TAKE_A_WHILE;

        Bot.EditMessage(Chat, message, text);

        var lines = threads.SelectMany(s => s).ToList();

        await EatMany(lines, name);
    }

    protected async Task EatMany(List<string> lines, string name)
    {
        await Baka_Eat_Report(lines, GetFileSavePath(name), _ => GetSourceAnnotation());
    }

    private string GetFileSavePath(string name)
    {
        var thread = BoardHelpers.FileNameIsThread(name);
        var date = thread
            ? $"{DateTime.Now:yyyy'-'MM'-'dd}"
            : $"{DateTime.Now:yyyy'-'MM'-'dd' 'HH'.'mm}";

        return ArchivePath
            .EnsureDirectoryExist()
            .Combine($"{date} {name.Replace(' ', '-')}.json");
    }

    protected Uri? UrlOrBust(ref string url)
    {
        try
        {
            if (url.Contains('/').Janai()) // is a board code e.g. "a" or "b"
            {
                var key = url;
                var board = Boards
                    .SelectMany(x => x.Boards)
                    .FirstOrDefault(x => x.Key == key);
                if (board != null) url = board.URL;
            }

            return new Uri(url);
        }
        catch
        {
            SetBadStatus();
            Bot.SendMessage(Origin, UnknownURL);
            return null;
        }
    }
}