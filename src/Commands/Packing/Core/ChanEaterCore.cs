using System.Text;
using Telegram.Bot.Types;
using Witlesss.Services.Internet.Boards;

namespace Witlesss.Commands.Packing.Core;

public abstract class ChanEaterCore : Fuse
{
    protected abstract string ArchivePath { get; }
    protected abstract string CallbackKey { get; }
    protected abstract string CommandName { get; }
    protected abstract string BoardsTitle { get; }
    protected abstract string Manual      { get; }
    protected abstract string UnknownURL  { get; }
    protected abstract string EmojiLogo   { get; }
    protected abstract string FileName    { get; }

    protected abstract List<BoardGroup> Boards { get; }

    protected abstract string? GetSourceAnnotation();

    protected override async Task RunAuthorized()
    {
        if (Args is null)
        {
            if (Command!.StartsWith($"/{CommandName}s"))
                SendBoardList(new ListPagination(Chat, PerPage: 2));
            else
                Bot.SendMessage(Chat, Manual);
        }
        else
        {
            if (Args.EndsWith("info"))
                SendSavedList(new ListPagination(Chat, PerPage: 10));
            else
                await EatBoard();
        }
    }

    public new void HandleCallback(CallbackQuery query, string[] data)
    {
        var pagination = query.GetPagination(data);

        if (data[0] == CallbackKey) SendBoardList(pagination);
        else                        SendSavedList(pagination);
    }

    private async Task EatBoard()
    {
        MeasureDick();
        GetWordsPerLineLimit();

        var args = Args.SplitN(2);
        var pair = args.Length > 1;
        var json = pair && args[0].Contains('-');
        if      (json) await EatJsonFile();
        else if (pair) await EatOnlineFind(args);
        else           await EatOnlineData(url: args[0]);
    }

    private async Task EatJsonFile()
    {
        var files = GetFiles(ArchivePath, $"{Args}.json");
        if (files.Length > 0)
        {
            await EatFromJsonFile(files[0]);
            GoodEnding();
        }
        else
            Bot.SendMessage(Chat, string.Format(FUSE_FAIL_BOARD, $"{CommandName}s"));
    }

    protected abstract Task EatOnlineData(string url);
    protected abstract Task EatOnlineFind(string[] args);

    protected async Task RespondAndStartEating(IEnumerable<Task<List<string>>> tasks)
    {
        var message = Bot.PingChat(Chat, BOARD_START);
        try
        {
            var threads = await Task.WhenAll(tasks);

            var text = string.Format(BOARD_START_EDIT, threads.Length);
            if (threads.Length > 150) text += MAY_TAKE_A_WHILE;

            Bot.EditMessage(Chat, message, text);

            var size = ChatService.GetPath(Chat).FileSizeInBytes();
            var lines = threads.SelectMany(s => s).ToList();

            await EatMany(lines, size, Limit);
        }
        catch
        {
            Bot.SendMessage(Chat, Bot.GetSillyErrorMessage());
        }
    }

    protected async Task EatMany(List<string> lines, long size, int limit)
    {
        var count = Baka.WordCount;

        await EatAllLines(lines, Baka, limit);
        SaveChanges(Baka, Title);

        JsonIO.SaveData(lines, GetFileSavePath());

        var report = FUSION_SUCCESS_REPORT(Baka, size, count, Title);
        var source = GetSourceAnnotation();
        if (source != null) report += source;

        Bot.SendMessage(Chat, report);
    }

    private string GetFileSavePath()
    {
        Directory.CreateDirectory(ArchivePath);

        var thread = BoardHelpers.FileNameIsThread(FileName);
        var date = thread
            ? $"{DateTime.Now:yyyy'-'MM'-'dd}"
            : $"{DateTime.Now:yyyy'-'MM'-'dd' 'HH'.'mm}";

        return Path.Combine(ArchivePath, $"{date} {FileName.Replace(' ', '-')}.json");
    }

    protected Uri UrlOrBust(ref string url)
    {
        try
        {
            if (url.Contains('/') == false) // is a board code e.g. "a" or "b"
            {
                var ending = $"/{url}/";
                var urls = Boards.SelectMany(x => x.Boards.Select(b => b.URL)).ToList();
                var match = urls.FirstOrDefault(x => x.EndsWith(ending));
                if (match != null)
                {
                    url = match;
                }
            }

            return new Uri(url);
        }
        catch
        {
            Bot.SendMessage(Chat, UnknownURL);
            throw;
        }
    }


    // LISTING

    private void SendBoardList(ListPagination pagination)
    {
        var (chat, messageId, page, perPage) = pagination;

        var boards = Boards.Skip(page * perPage).Take(perPage);
        var last = (int)Math.Ceiling(Boards.Count / (double)perPage) - 1;

        var sb = new StringBuilder(BoardsTitle).Append($" [PAGE: {page + 1}/{last + 1}]");
        foreach (var group in boards)
        {
            sb.Append($"\n\n<b><u>{group.Title}</u></b>");
            if (group.IsNSFW) sb.Append(" (NSFW🥵)");
            sb.Append('\n');
            foreach (var board in group.Boards)
            {
                sb.Append($"\n<i>{board.Title}</i>");
                if (board.IsNSFW) sb.Append(" (NSFW🥵)");
                sb.Append($" - <code>{board.URL}</code>");
            }
        }
        sb.Append(string.Format(BrowseReddit.SEARCH_FOOTER, Bot.Me.FirstName));
        sb.Append(USE_ARROWS);

        var text = sb.ToString();
        var buttons = GetPaginationKeyboard(page, perPage, last, CallbackKey);

        Bot.SendOrEditMessage(chat, text, messageId, buttons);
    }

    private void SendSavedList(ListPagination pagination)
    {
        var (chat, messageId, page, perPage) = pagination;

        var files = GetFilesInfo(ArchivePath).OrderByDescending(x => x.Name).ToArray();

        var paginated = files.Length > perPage;
        var lastPage = (int)Math.Ceiling(files.Length / (double)perPage) - 1;

        var sb = new StringBuilder(EmojiLogo).Append(" <b>Архив досокъ/трѣдовъ:</b> ");
        if (paginated) sb.Append($"📄[{page + 1}/{lastPage + 1}]");
        sb.Append("\n\n").AppendJoin('\n', BoardHelpers.GetJsonList(files, page, perPage));
        if (paginated) sb.Append(USE_ARROWS);

        var buttons = paginated ? GetPaginationKeyboard(page, perPage, lastPage, $"{CallbackKey}i") : null;
        Bot.SendOrEditMessage(chat, sb.ToString(), messageId, buttons);
    }
}