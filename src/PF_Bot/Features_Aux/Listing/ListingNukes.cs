using System.Text;
using PF_Bot.Core;
using PF_Bot.Features_Main.Memes.Core.Generators;
using PF_Bot.Features_Main.Memes.Core.Shared;

namespace PF_Bot.Features_Aux.Listing;

public static class ListingNukes // List of nuclear weapons tests - Wikipedia
{
    public static void SendNukeLog
        (ListPagination pagination)
    {
        var (origin, messageId, page, perPage) = pagination;

        if (DukeNukem.Logs.TryGetValue_Failed(origin.Chat, out var entries))
        {
            App.Bot.SendMessage(origin, NUKE_LOG_EXPLANATION);
            return;
        }

        var paginated = entries.Count > perPage;
        var lastPage = pagination.GetLastPageIndex(entries.Count);

        var sb = new StringBuilder("🍤 <b>Последние вариации /nuke:</b>");
        if (paginated) sb.Append($" 📃{page + 1}/{lastPage + 1}");
        sb.Append("\n\n").AppendJoin('\n', GetNukeLog(entries, page, perPage));
        sb.Append("\n\nИспользование: <code>/pegc [фильтр] .</code>");
        if (paginated) sb.Append(USE_ARROWS);

        var buttons = paginated
            ? pagination.GetPaginationKeyboard(lastPage, $"{Registry.CallbackKey_Nukes}l")
            : null;
        App.Bot.SendOrEditMessage(origin, sb.ToString(), messageId, buttons);
    }

    private static IEnumerable<string> GetNukeLog
        (List<DukeNukem.NukeLogEntry> entries, int page = 0, int perPage = 25)
    {
        if (entries.Count == 0) return [NUKE_LOG_EXPLANATION];

        return entries
            .OrderByDescending(x => x.Time)
            .Skip(perPage * page)
            .Take(perPage)
            .Select(entry =>
            {
                var logo = entry.Type switch
                {
                    MemeSourceType.Image => "📸",
                    MemeSourceType.Sticker => "🎟",
                    MemeSourceType.Video => "🎬",
                    _ => throw new ArgumentOutOfRangeException(),
                };
                return
                    $"{logo} <b>{entry.Time:MM'/'dd' 'HH:mm:ss}</b>\n"
                  + $"<blockquote><code>{entry.Command}</code></blockquote>";
            });
    }
}