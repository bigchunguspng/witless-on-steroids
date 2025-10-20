using PF_Bot.Core;
using PF_Bot.Features_Aux.Packs;
using PF_Bot.Features_Aux.Packs.Commands;
using PF_Bot.Features_Web.Reddit.Core;
using PF_Tools.Reddit;

namespace PF_Bot.Features_Web.Reddit.Commands;

public class EatReddit : Fuse
{
    // input: /xd [search query] [subreddit*] [-ops]
    protected override async Task RunAuthorized()
    {
        if (RedditHelpers.ParseArgs_SearchOrScroll(Args) is { } query)
        {
            MessageToEdit = Bot.PingChat(Origin, REDDIT_COMMENTS_START.Format(MAY_TAKE_A_WHILE));
            await EatComments(query);
        }
        else
            SendManual(REDDIT_COMMENTS_MANUAL);
    }

    private async Task EatComments(RedditQuery query)
    {
        RedditApp.Log("GET COMMENTS");
        var sw = Stopwatch.StartNew();
        var comments = await App.Reddit.GetComments(query);
        RedditApp.Log($"GET COMMENTS >> {sw.ElapsedReadable()}");

        await Baka_Eat_Report(comments, GetFileSavePath(query), report => GetDetails(report, query));
    }

    private string GetDetails(FeedReport report, RedditQuery query)
    {
        var subreddit
            = query is ScrollQuery scroll ? scroll.Subreddit
            : query is SearchQuery search ? search.Subreddit : null;
        var source  = subreddit != null
            ? $"<b>r/{subreddit}</b>"
            : "разных сабреддитов";
        return $"\n\nЕго пополнили {report.Consumed} комментов с {source}";
    }

    private string GetFileSavePath(RedditQuery query)
    {
        var date = $"{DateTime.Now:yyyy'-'MM'-'dd' 'HH'.'mm}";
        var name = query switch
        {
            ScrollQuery scroll => $"{scroll.Subreddit}",
            SearchQuery search => $"{search.Subreddit}_{search.Text.Replace(' ', '-')}",
            _ => throw new ArgumentOutOfRangeException(nameof(query)),
        };

        return Dir_History
            .EnsureDirectoryExist()
            .Combine($"{date} {name}.json");
    }
}