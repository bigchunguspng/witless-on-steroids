using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
#pragma warning disable CS4014

namespace Witlesss.Commands
{
    public class FuseRedditComments : WitlessCommand
    {
        private readonly Regex _que = new(@"^\/xd\S*\s((?:(?:.*)(?=\s[a-z0-9_]+\*))|(?:(?:[^\*]*)(?=\s-\S+))|(?:[^\*]*))(?!\S*\*)");
        private readonly Regex _sub = new(@"([a-z0-9_]+)\*");
        private readonly Regex _ops = new(@"(?<=-)([hntrc][hdwmya]?)\S*$");

        // input: /xd [search query] [subreddit*] [-ops]
        public override void Run()
        {
            if (Bot.ThorRagnarok.ChatIsBanned(Chat)) return;

            var input = RemoveBotMention();

            var que = _que.Match(input);
            var sub = _sub.Match(input);
            var ops = _ops.Match(input);

            if (que.Success || sub.Success)
            {
                var size = SizeInBytes(Baka.Path);

                var q = que.Success ? que.Groups[1].Value : null;
                var s = sub.Success ? sub.Groups[1].Value : null;
                var o = ops.Success ? ops.Value : que.Success ? "ra" : "ha";

                RedditQuery query;

                if (que.Success)
                {
                    var sort = CheckReddit.Sorts  [o[0]];
                    var time = CheckReddit.GetTime(o, CheckReddit.TimeMatters(o[0]));

                    query = sub.Success ? new SsQuery(s, q, sort, time) : new SrQuery(q, sort, time);
                }
                else
                {
                    var sort = (SortingMode)o[0];
                    var time = CheckReddit.GetTime(o, CheckReddit.TimeMatters(sort));

                    query = new ScQuery(s, sort, time);
                }

                var message = Bot.PingChat(Chat, REDDIT_COMMENTS_START);
                Bot.RunSafelyAsync(EatComments(SnapshotMessageData(), query, size), Chat, message);
            }
            else
            {
                Bot.SendMessage(Chat, REDDIT_COMMENTS_MANUAL, preview: false);
            }
        }

        private async Task EatComments(WitlessCommandParams x, RedditQuery query, long size)
        {
            var timer = new StopWatch();
            var comments = await RedditTool.Instance.GetComments(query);
            Log($"COMMENTS FETCHED >> {timer.CheckStopWatch()}");

            foreach (var text in comments) x.Baka.Eat(text);
            Log($"{x.Title} >> {LOG_FUSION_DONE}", ConsoleColor.Magenta);
            x.Baka.SaveNoMatterWhat();

            var newSize = SizeInBytes(x.Baka.Path);
            var difference = FileSize(newSize - size);
            var report = string.Format(FUSE_SUCCESS_RESPONSE, x.Title, FileSize(newSize), difference);
            var subreddit = query is ScQuery sc ? sc.Subreddit : query is SsQuery ss ? ss.Subreddit : null;
            subreddit = subreddit is not null ? $"<b>r/{subreddit}</b>" : "разных сабреддитов";
            var detais = $"\n\n Его пополнили {comments.Count} комментов с {subreddit}";
            Bot.SendMessage(x.Chat, report + detais);
        }
    }
}