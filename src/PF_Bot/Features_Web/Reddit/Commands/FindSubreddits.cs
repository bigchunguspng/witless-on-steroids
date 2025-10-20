using System.Text;
using PF_Bot.Core;
using PF_Bot.Features_Web.Reddit.Core;
using PF_Bot.Routing.Commands;
using Reddit.Controllers;

namespace PF_Bot.Features_Web.Reddit.Commands;

public class FindSubreddits : CommandHandlerAsync
{
    private static readonly RedditApp Reddit = App.Reddit;

    // input: /wss subreddit search query
    protected override async Task Run()
    {
        if (Args != null)
        {
            RedditApp.Log("/wss -> FIND SUBREDDITS");
            await SendSubredditList(Args);
        }
        else
        {
            Log($"{Title} >> REDDIT SUBS ?");
            SendManual(REDDIT_SUBS_MANUAL);
        }
    }

    private async Task SendSubredditList(string query)
    {
        var subs = await Reddit.FindSubreddits(query);
        var text = subs.Count > 0 
            ? GetSubredditList(query, subs) 
            : "<b>*пусто*</b>";

        Bot.SendMessage(Origin, text);
        Log($"{Title} >> FOUND {subs.Count} SUBS >> {query}");
    }

    private static string GetSubredditList(string query, List<Subreddit> subs)
    {
        var count_ED = subs.Count.ED("о", "а", "");
        var sb = new StringBuilder();
        sb.Append($"По запросу <b>{query}</b> найдено <b>{subs.Count}</b> сообществ{count_ED}:\n");
        foreach (var subreddit in subs)
        {
            var members = (subreddit.Subscribers ?? 0).Format_bruh_1k_100k_1M();
            sb.Append($"\n<code>{subreddit.Name}</code> - <i>{members}</i>");
        }

        return sb.Append("\n\nБлагодарим за использование поисковика ").Append(Bot.Me.FirstName).ToString();
    }
}