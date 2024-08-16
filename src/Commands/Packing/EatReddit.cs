using Witlesss.Services.Internet.Reddit;

#pragma warning disable CS4014

namespace Witlesss.Commands.Packing
{
    public class EatReddit : Fuse // todo uninherit it from fuse and make it witless async
    {
        private readonly Regex _que = new(@"((?:(?:.*)(?=\s[a-z0-9_]+\*))|(?:(?:[^\*]*)(?=\s-\S+))|(?:[^\*]*))(?!\S*\*)");
        private readonly Regex _sub = new(@"([a-z0-9_]+)\*");
        private readonly Regex _ops = new(@"(?<=-)([hntrc][hdwmya]?)\S*$");

        // input: /xd [search query] [subreddit*] [-ops]
        protected override void RunAuthorized()
        {
            var args = Args ?? "";
            var que = _que.Match(args);
            var sub = _sub.Match(args);
            var ops = _ops.Match(args);

            if (que.Success || sub.Success)
            {
                Baka.SaveChanges();
                Size = SizeInBytes(Baka.FilePath);

                GetWordsPerLineLimit();

                var q = que.GroupOrNull(1);
                var s = sub.GroupOrNull(1);
                var o = ops.Success ? ops.Value : que.Success ? "ra" : "ha";

                RedditQuery query;

                if (que.Success)
                {
                    var sort = BrowseReddit.Sorts  [o[0]];
                    var time = BrowseReddit.GetTime(o, BrowseReddit.TimeMatters(o[0]));

                    query = new SearchQuery(s, q, sort, time);
                }
                else
                {
                    var sort = (SortingMode)o[0];
                    var time = BrowseReddit.GetTime(o, BrowseReddit.TimeMatters(sort));

                    query = new ScrollQuery(s, sort, time);
                }

                var message = Bot.PingChat(Chat, string.Format(REDDIT_COMMENTS_START, MAY_TAKE_A_WHILE));
                Bot.RunSafelyAsync(EatComments(Context, query, Size, Limit), Chat, message);
            }
            else
            {
                Bot.SendMessage(Chat, REDDIT_COMMENTS_MANUAL);
            }
        }

        private async Task EatComments(WitlessContext c, RedditQuery query, long size, int limit)
        {
            var sw = GetStartedStopwatch();
            var comments = await RedditTool.Instance.GetComments(query);
            Log($"COMMENTS FETCHED >> {sw.ElapsedShort()}");

            EatAllLines(comments, c.Baka, limit, out var eated);
            SaveChanges(c.Baka, c.Title);

            var report = FUSION_SUCCESS_REPORT(c.Baka, size, c.Title);
            var subreddit = query is ScrollQuery sc ? sc.Subreddit : query is SearchQuery ss ? ss.Subreddit : null;
            subreddit = subreddit is not null ? $"<b>r/{subreddit}</b>" : "разных сабреддитов";
            var detais = $"\n\n Его пополнили {eated} комментов с {subreddit}";
            Bot.SendMessage(c.Chat, report + detais);
        }
    }
}