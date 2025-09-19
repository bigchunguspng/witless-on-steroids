using System.Text;
using PF_Bot.Backrooms.Helpers;
using PF_Bot.Core;
using PF_Bot.Core.Internet.Boards;
using PF_Bot.Handlers.Manage.Packs.Core;

namespace PF_Bot.Backrooms.Listing;

public static class ListingBoards
{
    public static void SendSavedList(ImageBoardContext ctx, ListPagination pagination)
    {
        var (origin, messageId, page, perPage) = pagination;

        var files = ctx.ArchivePath.GetFilesInfo()
            .Where(x => x.Length > 2)
            .OrderByDescending(x => x.Name).ToArray();

        var paginated = files.Length > perPage;
        var lastPage = (int)Math.Ceiling(files.Length / (double)perPage) - 1;

        var sb = new StringBuilder(ctx.EmojiLogo).Append(" <b>Архив досокъ/трѣдовъ:</b>");
        if (paginated) sb.Append($" 📃{page + 1}/{lastPage + 1}");
        sb.Append("\n\n").AppendJoin('\n', BoardHelpers.GetJsonList(files, page, perPage));
        if (paginated) sb.Append(USE_ARROWS);

        var buttons = paginated ? Listing.GetPaginationKeyboard(page, perPage, lastPage, $"{ctx.CallbackKey}i") : null;
        App.Bot.SendOrEditMessage(origin, sb.ToString(), messageId, buttons);
    }

    public static void SendBoardList(ImageBoardContext ctx, ListPagination pagination, List<BoardGroup> boardsAll)
    {
        var (origin, messageId, page, perPage) = pagination;

        var boards = boardsAll.Skip(page * perPage).Take(perPage);
        var last = (int)Math.Ceiling(boardsAll.Count / (double)perPage) - 1;

        var sb = new StringBuilder(ctx.BoardsTitle).Append($" 📃{page + 1}/{last + 1}");
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

        var buttons = Listing.GetPaginationKeyboard(page, perPage, last, ctx.CallbackKey);
        App.Bot.SendOrEditMessage(origin, sb.ToString(), messageId, buttons);
    }
}