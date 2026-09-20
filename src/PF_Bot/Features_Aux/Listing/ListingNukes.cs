using PF_Bot.Core;
using PF_Bot.Features_Main.Memes.Core.Generators;
using PF_Bot.Features_Main.Memes.Core.Shared;

namespace PF_Bot.Features_Aux.Listing;

public static class ListingNukes // List of nuclear weapons tests - Wikipedia
{
    public static void SendNukeLog
        (ListPagination pagination)
    {
        var origin = pagination.Origin;

        if (DukeNukem.Logs.TryGetValue_Failed(origin.Chat, out var entries))
        {
            App.Bot.SendMessage(origin, NUKE_LOG_EXPLANATION);
            return;
        }

        const string key = $"{Registry.CallbackKey_Nukes}l";
        Listing.SendList(entries, key, pagination, header: sb =>
        {
            sb.Append("🍤 <b>Последние вариации /nuke:</b>");
        }, itemText: (sb, entry) =>
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
        }, footer: sb =>
        {
            sb.Append("\n\nИспользование: <code>/pegc [фильтр] .</code>");
        }, placeholder: NUKE_LOG_EXPLANATION);
    }
}