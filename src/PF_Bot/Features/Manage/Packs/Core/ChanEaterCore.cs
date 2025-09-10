using System.Text;
using PF_Bot.Backrooms.Helpers;
using PF_Bot.Core.Chats;
using PF_Bot.Tools_Legacy.BoardScrappers;
using PF_Tools.Backrooms.Helpers;
using Telegram.Bot.Types;

namespace PF_Bot.Features.Manage.Packs.Core;

public abstract class ChanEaterCore : Fuse
{
    protected abstract FilePath ArchivePath { get; }

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
                SendBoardList(new ListPagination(Origin, PerPage: 2));
            else
                Bot.SendMessage(Origin, Manual);
        }
        else
        {
            if (Args.EndsWith("info"))
                SendSavedList(new ListPagination(Origin, PerPage: 10));
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
        var files = ArchivePath.GetFiles($"{Args}.json");
        if (files.Length > 0)
        {
            await EatFromJsonFile(files[0]);
            GoodEnding();
        }
        else
            Bot.SendMessage(Origin, string.Format(FUSE_FAIL_BOARD, $"{CommandName}s"));
    }

    protected abstract Task EatOnlineData(string url);
    protected abstract Task EatOnlineFind(string[] args);

    protected async Task RespondAndStartEating(IEnumerable<Task<List<string>>> tasks)
    {
        var message = Bot.PingChat(Origin, BOARD_START);

        var threads = await Task.WhenAll(tasks);

        var text = string.Format(BOARD_START_EDIT, threads.Length);
        if (threads.Length > 150) text += MAY_TAKE_A_WHILE;

        Bot.EditMessage(Chat, message, text);

        var size = ChatManager.GetPackPath(Chat).FileSizeInBytes;
        var lines = threads.SelectMany(s => s).ToList();

        await EatMany(lines, size, Limit);
    }

    protected async Task EatMany(List<string> lines, long size, int limit)
    {
        var count = Baka.VocabularyCount;

        await EatAllLines(lines, Baka, limit);
        SaveChanges(Baka, Chat, Title);

        JsonIO.SaveData(lines, GetFileSavePath());

        var report = FUSION_SUCCESS_REPORT(Baka, Chat, size, count, Title);
        var source = GetSourceAnnotation();
        if (source != null) report += source;

        Bot.SendMessage(Origin, report);
    }

    private string GetFileSavePath()
    {
        var thread = BoardHelpers.FileNameIsThread(FileName);
        var date = thread
            ? $"{DateTime.Now:yyyy'-'MM'-'dd}"
            : $"{DateTime.Now:yyyy'-'MM'-'dd' 'HH'.'mm}";

        return ArchivePath
            .EnsureDirectoryExist()
            .Combine($"{date} {FileName.Replace(' ', '-')}.json");
    }

    protected Uri UrlOrBust(ref string url)
    {
        try
        {
            if (url.Contains('/') == false) // is a board code e.g. "a" or "b"
            {
                var key = url;
                var match = Boards.SelectMany(x => x.Boards).FirstOrDefault(x => x.Key == key);
                if (match != null)
                {
                    url = match.URL;
                }
            }

            return new Uri(url);
        }
        catch
        {
            Bot.SendMessage(Origin, UnknownURL);
            throw;
        }
    }


    // LISTING

    private void SendBoardList(ListPagination pagination)
    {
        var (origin, messageId, page, perPage) = pagination;

        var boards = Boards.Skip(page * perPage).Take(perPage);
        var last = (int)Math.Ceiling(Boards.Count / (double)perPage) - 1;

        var sb = new StringBuilder(BoardsTitle).Append($" 📃{page + 1}/{last + 1}");
        foreach (var group in boards)
        {
            sb.Append($"\n\n<b><u>{group.Title}</u></b>");
            if (group.IsNSFW) sb.Append(" (NSFW🥵)");
            sb.Append('\n');
            foreach (var board in group.Boards)
            {
                sb.Append(board.Key is null ? "\n\n" : $"\n<code>{board.Key}</code> - ");
                sb.Append($"<i><a href='{board.URL}'>{board.Title}</a></i>");
                if (board.IsNSFW) sb.Append(" (NSFW🥵)");
            }
        }
        sb.Append(USE_ARROWS);

        var text = sb.ToString();
        var buttons = GetPaginationKeyboard(page, perPage, last, CallbackKey);

        Bot.SendOrEditMessage(origin, text, messageId, buttons);
    }

    private void SendSavedList(ListPagination pagination)
    {
        var (origin, messageId, page, perPage) = pagination;

        var files = ArchivePath.GetFilesInfo()
            .Where(x => x.Length > 2)
            .OrderByDescending(x => x.Name).ToArray();

        var paginated = files.Length > perPage;
        var lastPage = (int)Math.Ceiling(files.Length / (double)perPage) - 1;

        var sb = new StringBuilder(EmojiLogo).Append(" <b>Архив досокъ/трѣдовъ:</b>");
        if (paginated) sb.Append($" 📃{page + 1}/{lastPage + 1}");
        sb.Append("\n\n").AppendJoin('\n', BoardHelpers.GetJsonList(files, page, perPage));
        if (paginated) sb.Append(USE_ARROWS);

        var buttons = paginated ? GetPaginationKeyboard(page, perPage, lastPage, $"{CallbackKey}i") : null;
        Bot.SendOrEditMessage(origin, sb.ToString(), messageId, buttons);
    }
}