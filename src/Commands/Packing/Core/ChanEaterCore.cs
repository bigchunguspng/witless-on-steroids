using System.Text;
using Witlesss.Services.Internet.Boards;

namespace Witlesss.Commands.Packing.Core;

public abstract class ChanEaterCore : Fuse
{
    protected abstract string ArchivePath   { get; }
    protected abstract string CommandString { get; }
    protected abstract string Manual        { get; }
    protected abstract string FileName      { get; }

    protected abstract List<BoardGroup> Boards { get; }

    protected abstract string? GetSourceAnnotation();

    protected override async Task RunAuthorized()
    {
        if (Args is null)
        {
            if (Command!.StartsWith($"/{CommandString}s"))
                SendBoardList(new ListPagination(Chat, PerPage: 2), Boards);
            else
                Bot.SendMessage(Chat, Manual);
        }
        else
        {
            if (Args.EndsWith("info"))
                SendSavedList(new ListPagination(Chat, PerPage: 10), ArchivePath);
            else
                await EatBoard();
        }
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
            Bot.SendMessage(Chat, string.Format(FUSE_FAIL_BOARD, $"{CommandString}s"));
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
        Directory.CreateDirectory(Dir_Board);

        var thread = BoardHelpers.FileNameIsThread(FileName);
        var date = thread
            ? $"{DateTime.Now:yyyy'-'MM'-'dd}"
            : $"{DateTime.Now:yyyy'-'MM'-'dd' 'HH'.'mm}";

        return Path.Combine(Dir_Board, $"{date} {FileName}.json");
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
            Bot.SendMessage(Chat, "Dude, wrong URL 👉😄");
            throw;
        }
    }


    // LISTING

    protected static void SendBoardList(ListPagination pagination, List<BoardGroup> allBoards) // todo NSFW boards
    {
        var (chat, messageId, page, perPage) = pagination;

        var boards = allBoards.Skip(page * perPage).Take(perPage);
        var last = (int)Math.Ceiling(allBoards.Count / (double)perPage) - 1;

        var sb = new StringBuilder("🍀🍀🍀 <b>4CHAN BOARDS</b> 🍀🍀🍀");
        sb.Append(" [PAGE: ").Append(page + 1).Append('/').Append(last + 1).Append(']');
        foreach (var group in boards)
        {
            sb.Append($"\n\n<b><u>{group.Title}</u></b>");
            if (group.IsNSFW) sb.Append(" (NSFW🥵)");
            sb.Append('\n');
            foreach (var board in group.Boards)
            {
                sb.Append($"\n<i>{board.Title}</i> - <code>{board.URL}</code>");
            }
        }
        sb.Append(string.Format(BrowseReddit.SEARCH_FOOTER, Bot.Me.FirstName));
        sb.Append(USE_ARROWS);

        var text = sb.ToString();
        var buttons = GetPaginationKeyboard(page, perPage, last, "b");

        Bot.SendOrEditMessage(chat, text, messageId, buttons);
    }

    protected static void SendSavedList(ListPagination pagination, string directory)
    {
        var (chat, messageId, page, perPage) = pagination;

        var files = GetFilesInfo(directory).OrderByDescending(x => x.Name).ToArray();

        var single = files.Length <= perPage;

        var lastPage = (int)Math.Ceiling(files.Length / (double)perPage) - 1;
        var sb = new StringBuilder("🍀 <b>Архив досокъ/трѣдовъ:</b> ");
        if (!single) sb.Append("📄[").Append(page + 1).Append('/').Append(lastPage + 1).Append(']');
        sb.Append("\n\n").AppendJoin('\n', BoardHelpers.GetJsonList(files, page, perPage));
        if (!single) sb.Append(USE_ARROWS);

        var buttons = single ? null : GetPaginationKeyboard(page, perPage, lastPage, "bi");
        Bot.SendOrEditMessage(chat, sb.ToString(), messageId, buttons);
    }
}