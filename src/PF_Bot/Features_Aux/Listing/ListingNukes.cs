using PF_Bot.Core;
using PF_Bot.Features_Main.Memes.Core.Generators;
using PF_Bot.Features_Main.Memes.Core.Shared;

namespace PF_Bot.Features_Aux.Listing;

public static class ListingNukes // List of nuclear weapons tests - Wikipedia
{
    public static bool SendNukeLog
        (ListPagination pagination)
    {
        var origin = pagination.Origin;

        if (DukeNukem.Logs.TryGetValue_Failed(origin.Chat, out var entries))
        {
            App.Bot.SendMessage(origin, NUKE_LOG_EXPLANATION);
            return false;
        }

        const string key = $"{Registry.CallbackKey_Nukes}l";
        new PaginatedList<DukeNukem.NukeLogEntry>(entries, key, pagination)
        {
            Header = sb => sb.Append("🍤 <b>Последние вариации /nuke:</b>"),
            ItemText = (sb, entry) =>
            {
                var logo = entry.Type switch
                {
                    MemeSourceType.Image => "📸",
                    MemeSourceType.Sticker => "🎟",
                    MemeSourceType.Video => "🎬",
                    _ => throw new ArgumentOutOfRangeException(),
                };
                sb.Append($"{logo} <b>{entry.Time:MM'/'dd' 'HH:mm:ss}</b>\n");
                sb.Append($"<blockquote><code>{entry.Command}</code></blockquote>");
            },
            Footer = sb => sb.Append("\n\nИспользование: <code>/pegc [фильтр] .</code>"),
            Placeholder = NUKE_LOG_EXPLANATION,
        }.Send();
        return true;
    }
}