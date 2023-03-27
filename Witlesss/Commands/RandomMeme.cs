using System.Net;
using System.Text.RegularExpressions;
using Telegram.Bot.Types.InputFiles;

#pragma warning disable CS8509
#pragma warning disable SYSLIB0014

namespace Witlesss.Commands
{
    public class CheckReddit : Command
    {
        private readonly Regex _sub = new(@"(_|\s)([a-z0-9_]+)");
        private readonly Regex _ops = new(@"-([hntrc][hdwmya]?)");
        private readonly Regex _rep = new(@"^\/w[^\s_@]");

        private RedditTool Reddit => Bot.Reddit;
        
        // input: /w[@piece_fap_club][_subreddit [-h/-n/-t/-c/-ta/...]]
        public override void Run()
        {
            if (Bot.ThorRagnarok.ChatIsBanned(Chat)) return;
            
            var input = Text.ToLower().Replace(Config.BotUsername, "");

            if (Message.ReplyToMessage is { Text: { } t } m && t.ToLower().StartsWith("/w"))
            {
                Pass(m);
                Run(); // RECURSIVE
            }
            else if (_rep.IsMatch(input))
            {
                Log("LAST QUERY");
                SendPost(Reddit.LastQueryOrRandom(Chat));
            }
            else
            {
                var sub = _sub.Match(input);
                if (sub.Success)
                {
                    var subreddit = sub.Groups[2].Value;
                    var ops = _ops.Match(input);
                    if (ops.Success)
                    {
                        var options = ops.Groups[1].Value;
                        var sorting = GetSortingMode(options);
                        var period = GetTimePeriod(options, sorting);
                        
                        Log("SUBREDDIT + OPTIONS");
                        SendPost(new SrQuery(subreddit, sorting, period));
                    }
                    else
                    {
                        Log("SUBREDDIT");
                        SendPost(new SrQuery(subreddit));
                    }
                }
                else
                {
                    Log("DEFAULT (RANDOM)");
                    SendPost(Reddit.RandomSubQuery);
                }
            }
        }

        private void SendPost(SrQuery query)
        {
            var post = TryToGetPost(query);
            if (post == null) return;

            try
            {
                var file = new InputOnlineFile(post.URL);
                var b = post.URL.EndsWith(".gif");
                if (b) Bot.SendAnimaXD(Chat, file, post.Title);
                else   Bot.SendPhotoXD(Chat, file, post.Title);
            }
            catch
            {
                var path = Bot.MemeService.Compress(DownloadMeme(post));
                using var stream = File.OpenRead(path);
                Bot.SendPhotoXD(Chat, new InputOnlineFile(stream), post.Title);
            }
            finally // jerk it
            {
                Log($"{Title} >> r/{post.Subreddit}");
                Reddit.LogInfo();
            }
        }

        private PostData TryToGetPost(SrQuery query)
        {
            try
            {
                return Reddit.PullPost(query, Chat);
            }
            catch
            {
                Bot.SendMessage(Chat, "💀");
                return null;
            }
        }

        private static string DownloadMeme(PostData post)
        {
            var name = UniquePath($@"{PICTURES_FOLDER}\{post.Fullname}.png");
            using var web = new WebClient();
            web.DownloadFile(post.URL, name);

            return name;
        }

        private static SortingMode GetSortingMode(string options) => options[0] switch
        {
            'h' => SortingMode.Hot,
            'n' => SortingMode.New,
            't' => SortingMode.Top,
            'r' => SortingMode.Rising,
            'c' => SortingMode.Controversial
        };

        private static string GetTimePeriod(string options, SortingMode sorting)
        {
            return options.Length == 1 || SortingIsTimed(sorting) ? "all" : GetTimePeriod(options[1]);
        }
        private static string GetTimePeriod(char c) => c switch
        {
            'h' => "hour",
            'd' => "day",
            'w' => "week",
            'm' => "month",
            'y' => "year",
            _   => "all"
        };
        private static bool SortingIsTimed(SortingMode s) => s is not SortingMode.Top and not SortingMode.Controversial;
    }

    public class GetRedditLink : Command
    {
        private RedditTool Reddit => Bot.Reddit;
        
        public override void Run()
        {
            if (Message.ReplyToMessage is { } message)
            {
                Pass(message);
                if (Reddit.Recall(Text) is { } post)
                {
                    Bot.SendMessage(Chat, $"<b><a href='{post.Permalink}'>r/{post.Subreddit}</a></b>", preview: false);
                }
                else
                {
                    Bot.SendMessage(Chat, $"{Pick(I_FORGOR_RESPONSE)} {Pick(FAIL_EMOJI)}");
                }
            }
            else Bot.SendMessage(Chat,  string.Format(LINK_MANUAL, RedditTool.KEEP_POSTS));
        }
    }
}