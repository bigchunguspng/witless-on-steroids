using System.Text;
using PF_Bot.Core;
using PF_Bot.Handlers.Edit.Direct;

namespace PF_Bot.Backrooms.Listing;

public static class ListingAliases
{
    public static void SendList(AliasContext ctx, ListPagination pagination)
    {
        var (origin, messageId, page, perPage) = pagination;

        var files = ctx.Directory.GetFiles();

        var single = files.Length <= perPage;

        var lastPage = (int)Math.Ceiling(files.Length / (double)perPage) - 1;
        var sb = new StringBuilder("🔥 <b>Ярлыки команды /").Append(ctx.CommandName).Append(":</b>");
        if (single.Janai()) sb.Append($" 📃{page + 1}/{lastPage + 1}");
        sb.Append("\n\n").AppendJoin('\n', GetList(files, page, perPage));
        if (single.Janai()) sb.Append(USE_ARROWS);

        var buttons = single ? null : Listing.GetPaginationKeyboard(page, perPage, lastPage, ctx.CallbackKey);
        App.Bot.SendOrEditMessage(origin, sb.ToString(), messageId, buttons);
    }

    private static IEnumerable<string> GetList(string[] files, int page = 0, int perPage = 25)
    {
        if (files.Length == 0)
        {
            yield return "*пусто*";
            yield break;
        }

        foreach (var file in files.Skip(page * perPage).Take(perPage))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            yield return $"<code>{name}</code>:\n<blockquote>{File.ReadAllText(file)}</blockquote>";
        }
    }
}