using System.Text;
using PF_Bot.Core;
using PF_Bot.Core.Memes.Generators;
using PF_Bot.Core.Memes.Shared;
using PF_Bot.Routing_New.Routers;

namespace PF_Bot.Backrooms.Listing;

public static class ListingNukes // List of nuclear weapons tests - Wikipedia
{
    public static void SendNukeLog(ListPagination pagination)
    {
        var (origin, messageId, page, perPage) = pagination;

        if (DukeNukem.Logs.TryGetValue_Failed(origin.Chat, out var entries))
        {
            App.Bot.SendMessage(origin, NUKE_LOG_EXPLANATION);
            return;
        }

        var single = entries.Count <= perPage;

        var lastPage = (int)Math.Ceiling(entries.Count / (double)perPage) - 1;
        var sb = new StringBuilder("🍤 <b>Последние вариации /nuke:</b>");
        if (single.Janai()) sb.Append($" 📃{page + 1}/{lastPage + 1}");
        sb.Append("\n\n").AppendJoin('\n', GetNukeLog(entries, page, perPage));
        sb.Append("\n\nИспользование: <code>/pegc [фильтр] .</code>");
        if (single.Janai()) sb.Append(USE_ARROWS);

        var buttons = single ? null : Listing.GetPaginationKeyboard(page, perPage, lastPage, $"{CallbackRouter_Default.Key_Nukes}l");
        App.Bot.SendOrEditMessage(origin, sb.ToString(), messageId, buttons);
    }

    private static IEnumerable<string> GetNukeLog(List<DukeNukem.NukeLogEntry> entries, int page = 0, int perPage = 25)
    {
        if (entries.Count == 0)
        {
            yield return NUKE_LOG_EXPLANATION;
            yield break;
        }

        foreach (var entry in entries.OrderByDescending(x => x.Time).Skip(page * perPage).Take(perPage))
        {
            var logo = entry.Type switch
            {
                MemeSourceType.Image => "📸",
                MemeSourceType.Sticker => "🎟",
                MemeSourceType.Video => "🎬",
                _ => throw new ArgumentOutOfRangeException(),
            };
            yield return $"{logo} <b>{entry.Time:MM'/'dd' 'HH:mm:ss}</b>\n<blockquote><code>{entry.Command}</code></blockquote>";
        }
    }
}