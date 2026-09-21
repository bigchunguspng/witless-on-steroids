using PF_Bot.Features_Main.Edit.Commands.Manual;

namespace PF_Bot.Features_Aux.Listing;

public static class ListingAliases
{
    public static void SendList
        (AliasContext ctx, ListPagination pagination)
    {
        new PaginatedList<string>(ctx.Directory.GetFiles(), ctx.CallbackKey, pagination)
        {
            Header = sb => sb.Append("🔥 <b>Ярлыки команды /").Append(ctx.CommandName).Append(":</b>"),
            ItemText = (sb, file) =>
            {
                var name    = Path.GetFileNameWithoutExtension(file);
                var content = File.ReadAllText(file);
                if (ctx.ShowFlairs) sb.Append(GetAliasKindEmoji(content)).Append(' ');
                sb.Append($"<code>{name}</code>:\n");
                sb.Append($"<blockquote>{content}</blockquote>");
            },
        }.Send();
    }

    private static string GetAliasKindEmoji(string text) // ffmpeg only
    {
        if (text.StartsWith('-')) return "🔩";
        if (text.StartsWith('[')) return "🎞️";

        var eq_i = text.IndexOf('=');
        if (eq_i > 0)
        {
            var filter_name = text.Substring(0, eq_i);
            if (ListingFFMpegDocs.Docs.PagesAF.Any(x => x.Title.Contains(filter_name))) return "🎧";
            if (ListingFFMpegDocs.Docs.PagesVF.Any(x => x.Title.Contains(filter_name))) return "🎬";
        }

        return "💬";
    }
}