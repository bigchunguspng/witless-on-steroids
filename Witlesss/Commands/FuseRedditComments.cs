using System.Text.RegularExpressions;
using System.Threading.Tasks;

#pragma warning disable CS4014

namespace Witlesss.Commands
{
    public class FuseRedditComments : Fuse
    {
        private readonly Regex _que = new(@"^\/xd\S*\s((?:(?:.*)(?=\s[a-z0-9_]+\*))|(?:(?:[^\*]*)(?=\s-\S+))|(?:[^\*]*))(?!\S*\*)");
        private readonly Regex _sub = new(@"([a-z0-9_]+)\*");
        private readonly Regex _ops = new(@"(?<=-)([hntrc][hdwmya]?)\S*$");

        // input: /xd [search query] [subreddit*] [-ops]
        protected override void ExecuteAuthorized()
        {
            if (Bot.ThorRagnarok.ChatIsBanned(Chat)) return;

            var input = TextWithoutBotUsername;

            var que = _que.Match(input);
            var sub = _sub.Match(input);
            var ops = _ops.Match(input);

            if (que.Success || sub.Success)
            {
                Baka.Save();
                Size = SizeInBytes(Baka.Path);

                GetWordsPerLineLimit();

                var q = que.Success ? que.Groups[1].Value : null;
                var s = sub.Success ? sub.Groups[1].Value : null;
                var o = ops.Success ? ops.Value : que.Success ? "ra" : "ha";

                RedditQuery query;

                if (que.Success)
                {
                    var sort = BrowseReddit.Sorts  [o[0]];
                    var time = BrowseReddit.GetTime(o, BrowseReddit.TimeMatters(o[0]));

                    query = sub.Success ? new SsQuery(s, q, sort, time) : new SrQuery(q, sort, time);
                }
                else
                {
                    var sort = (SortingMode)o[0];
                    var time = BrowseReddit.GetTime(o, BrowseReddit.TimeMatters(sort));

                    query = new ScQuery(s, sort, time);
                }

                var message = Bot.PingChat(Chat, string.Format(REDDIT_COMMENTS_START, MAY_TAKE_A_WHILE));
                Bot.RunSafelyAsync(EatComments(SnapshotMessageData(), query, Size, Limit), Chat, message);
            }
            else
            {
                Bot.SendMessage(Chat, REDDIT_COMMENTS_MANUAL, preview: false);
            }
        }

        private async Task EatComments(WitlessMessageData x, RedditQuery query, long size, int limit)
        {
            var timer = new Stopwatch();
            var comments = await RedditTool.Instance.GetComments(query);
            Log($"COMMENTS FETCHED >> {timer.CheckElapsed()}");

            EatAllLines(comments, x.Baka, limit, out var eated);
            SaveChanges(x.Baka, x.Title);

            var report = FUSION_SUCCESS_REPORT(x.Baka, size, x.Title);
            var subreddit = query is ScQuery sc ? sc.Subreddit : query is SsQuery ss ? ss.Subreddit : null;
            subreddit = subreddit is not null ? $"<b>r/{subreddit}</b>" : "разных сабреддитов";
            var detais = $"\n\n Его пополнили {eated} комментов с {subreddit}";
            Bot.SendMessage(x.Chat, report + detais);
        }
    }
}