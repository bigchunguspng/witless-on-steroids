using PF_Bot.Features.Media.Reddit;
using PF_Bot.Tools_Legacy.RedditSearch;
using PF_Tools.Backrooms.Helpers;

#pragma warning disable CS4014

namespace PF_Bot.Features.Manage.Packs
{
    public class EatReddit : Fuse
    {
        private readonly Regex _que = new(@"((?:(?:.*)(?=\s[a-z0-9_]+\*))|(?:(?:[^\*]*)(?=\s-\S+))|(?:[^\*]*))(?!\S*\*)");
        private readonly Regex _sub = new(@"([a-z0-9_]+)\*");
        private readonly Regex _ops = new(@"(?<=-)([hntrc][hdwmya]?)\S*$");

        // input: /xd [search query] [subreddit*] [-ops]
        protected override async Task RunAuthorized()
        {
            var args = Args ?? "";
            var que = _que.Match(args);
            var sub = _sub.Match(args);
            var ops = _ops.Match(args);

            var queSuccess = que.Success && !string.IsNullOrWhiteSpace(que.Groups[1].Value);

            if (Args != null && (queSuccess || sub.Success))
            {
                MeasureDick();
                GetWordsPerLineLimit();

                var q = que.GroupOrNull(1);
                var s = sub.GroupOrNull(1);
                var o = ops.Success ? ops.Value : queSuccess ? "ra" : "ha";

                RedditQuery query;

                if (queSuccess)
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

                var message = Bot.PingChat(Origin, string.Format(REDDIT_COMMENTS_START, MAY_TAKE_A_WHILE));
                await Bot.RunOrThrow(EatComments(query, Size, Limit), Chat, message);
            }
            else
            {
                Bot.SendMessage(Origin, REDDIT_COMMENTS_MANUAL);
            }
        }

        private async Task EatComments(RedditQuery query, long size, int limit)
        {
            var sw = Stopwatch_StartNew();
            var comments = await RedditTool.Queue.Enqueue(() => RedditTool.Instance.GetComments(query));
            Log($"COMMENTS FETCHED >> {sw.ElapsedReadable()}");

            var count = Baka.VocabularyCount;

            var commentsEaten = await EatAllLines(comments, Baka, limit);
            SaveChanges(Baka, Chat, Title);

            JsonIO.SaveData(comments, GetFileSavePath(query));

            var report = FUSION_SUCCESS_REPORT(Baka, Chat, size, count, Title);
            var subreddit = query is ScrollQuery sc ? sc.Subreddit : query is SearchQuery ss ? ss.Subreddit : null;
            subreddit = subreddit is not null ? $"<b>r/{subreddit}</b>" : "разных сабреддитов";
            var detais = $"\n\nЕго пополнили {commentsEaten} комментов с {subreddit}";
            Bot.SendMessage(Origin, report + detais);
        }

        private string GetFileSavePath(RedditQuery query)
        {
            Directory.CreateDirectory(Dir_History);

            var date = $"{DateTime.Now:yyyy'-'MM'-'dd' 'HH'.'mm}";
            var name = query switch
            {
                ScrollQuery sc => $"{sc.Subreddit}",
                SearchQuery se => $"{se.Subreddit}_{se.Q.Replace(' ', '-')}",
                _ => throw new ArgumentOutOfRangeException(nameof(query))
            };

            return Path.Combine(Dir_History, $"{date} {name}.json");
        }
    }
}