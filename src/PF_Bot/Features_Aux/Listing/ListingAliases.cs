using System.Text;
using PF_Bot.Core;
using PF_Bot.Features_Main.Edit.Commands.Manual;

namespace PF_Bot.Features_Aux.Listing;

public static class ListingAliases
{
    public static void SendList
        (AliasContext ctx, ListPagination pagination)
    {
        var (origin, messageId, page, perPage) = pagination;

        var files = ctx.Directory.GetFiles();

        var paginated = files.Length > perPage;
        var lastPage = pagination.GetLastPageIndex(files.Length);

        var sb = new StringBuilder("🔥 <b>Ярлыки команды /").Append(ctx.CommandName).Append(":</b>");
        if (paginated) sb.Append($" 📃{page + 1}/{lastPage + 1}");
        sb.Append("\n\n").AppendJoin('\n', GetList(files, page, perPage));
        if (paginated) sb.Append(USE_ARROWS);

        var buttons = paginated
            ? pagination.GetPaginationKeyboard(lastPage, ctx.CallbackKey)
            : null;
        App.Bot.SendOrEditMessage(origin, sb.ToString(), messageId, buttons);
    }

    private static IEnumerable<string> GetList
        (string[] files, int page = 0, int perPage = 25)
    {
        if (files.Length == 0) return ["*пусто*"];

        return files
            .Skip(perPage * page)
            .Take(perPage)
            .Select(file =>
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var content = File.ReadAllText(file);
                return
                    $"<code>{name}</code>:\n"
                  + $"<blockquote>{content}</blockquote>";
            });
    }
}