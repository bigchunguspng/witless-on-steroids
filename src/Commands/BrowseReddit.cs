using System.Diagnostics;
using System.Net;
using System.Text;
using Reddit.Controllers;
using Telegram.Bot.Types;
using Witlesss.Services.Internet.Reddit;

#pragma warning disable CS8509
#pragma warning disable SYSLIB0014

namespace Witlesss.Commands // ReSharper disable InconsistentNaming
{
    public class BrowseReddit : AsyncCommand
    {
        private static readonly Regex _arg = new(@"((?:(?:.+)(?=\s[a-z0-9_]+\*))|(?:(?:.+)(?=\s-\S+))|(?:.+))");
        private static readonly Regex _sub = new(@"([a-z0-9_]+)");
        private static readonly Regex sub_ = new(@"([a-z0-9_]+)\*");
        private static readonly Regex _ops = new(@"(?<=-)([hntrc][hdwmya]?)\S*$");
        private static readonly Regex _wtf = new(@"^\/w[^\ss_@]");

        private static readonly RedditTool Reddit = RedditTool.Instance;

        // input: /w {reply message}
        // input: /ww
        // input: /wss subreddit
        // input: /ws [subreddit [-ops]]
        // input: /w search query [subreddit*] [-ops]   (ops: -h/-n/-t/-c/-ta/...)
        protected override async Task Run()
        {
            await Task.Delay(50); // to trigger async-ness

            if (Message.ReplyToMessage is { Text: { } t } message && IsCommand(t, "/w"))
            {
                Context = CommandContext.FromMessage(message);
                await Run(); // RECURSIVE
            }
            else if (_wtf.IsMatch(Command!))
            {
                LogDebug("LAST QUERY");
                await RedditTool.Queue.Enqueue(() => SendPost(Reddit.GetLastOrRandomQuery(Chat)));
            }
            else if (Command!.StartsWith("/wss")) // subreddit
            {
                if (Args is not null)
                {
                    var subs = await RedditTool.Queue.Enqueue(() => Reddit.FindSubreddits(Args));
                    var b = subs.Count > 0;

                    Bot.SendMessage(Chat, b ? SubredditList(Args, subs) : "<b>*пусто*</b>");
                    Log($"{Title} >> FIND {subs.Count} SUBS >> {Args}");
                }
                else
                {
                    Bot.SendMessage(Chat, REDDIT_SUBS_MANUAL);
                }
            }
            else if (Command!.StartsWith("/ws")) // [subreddit [-ops]]
            {
                var sub = _sub.Match(Args ?? "");
                if (sub.Success)
                {
                    var subreddit = sub.Groups[1].Value;

                    var options = GetOptions("ha");

                    var sort = (SortingMode)options[0];
                    var time = GetTime(options, TimeMatters(sort));

                    LogDebug("SUBREDDIT");
                    await RedditTool.Queue.Enqueue(() => SendPost(new ScrollQuery(subreddit, sort, time)));
                }
                else
                {
                    Bot.SendMessage(Chat, REDDIT_MANUAL);
                }
            }
            else // /w search query [subreddit*] [-ops]
            {
                var arg = _arg.Match(Args ?? "");
                if (arg.Success)
                {
                    var q = arg.Groups[1].Value;

                    var subreddit = sub_.ExtractGroup(1, Args ?? "", s => s);

                    var options = GetOptions("ra");

                    var sort = Sorts  [options[0]];
                    var time = GetTime(options, TimeMatters(options[0]));

                    LogDebug("SEARCH");
                    await RedditTool.Queue.Enqueue(() => SendPost(new SearchQuery(subreddit, q, sort, time)));
                }
                else
                {
                    LogDebug("DEFAULT (RANDOM)");
                    await RedditTool.Queue.Enqueue(() => SendPost(Reddit.RandomSubredditQuery));
                }
            }

            bool IsCommand(string a, string b) => a.ToLower().StartsWith(b);

            string GetOptions(string alt) => _ops.ExtractGroup(0, Args ?? "", s => s, alt)!;
        }

        public  static string GetTime(string o, bool b) => o.Length > 1 && b ? Times[o[1]] : Times['a'];

        public  static readonly Dictionary<char, string> Sorts = new()
        {
            { 'r', "relevance" }, { 'h', "hot" }, { 't', "top" }, { 'n', "new" }, { 'c', "comments" }
        };
        private static readonly Dictionary<char, string> Times = new()
        {
            { 'a', "all" }, { 'h', "hour" }, { 'd', "day" }, { 'w', "week" }, { 'm', "month" }, { 'y', "year" }
        };
        
        public  static bool TimeMatters(SortingMode s) => s is SortingMode.Top or SortingMode.Controversial;
        public  static bool TimeMatters(char        c) => c is not 'h' and not 'n';

        #region SENDING MEMES

        private void SendPost(RedditQuery query)
        {
            var post = GetPostOrBust(query);
            if (post == null) return;

            var a = post.URL.Contains("/gallery/");
            if (a)  SendGalleryPost(post);
            else SendSingleFilePost(post);

            if (ChatService.BakaIsLoaded(Chat, out var baka)) baka.Eat(post.Title);

            Log($"{Title} >> r/{post.Subreddit} (Q:{Reddit.QueriesCached} P:{Reddit.PostsCached})");
        }

        private void SendGalleryPost(PostData post) => Bot.SendAlbum(Chat, AlbumFromGallery(post));

        private static IEnumerable<InputMediaPhoto> AlbumFromGallery(PostData post)
        {
            var process = StartGalleryDL(post.URL);
            var urls = new List<string>();
            for (var i = 0; i < 10; i++)
            {
                var line = process.StandardOutput.ReadLine();
                if (line is null) break;

                urls.Add(line);
            }

            var captioned = false;
            return urls.Select(GetInputMedia);

            InputMediaPhoto GetInputMedia(string url)
            {
                var caption = captioned ? null : post.Title;
                captioned = true;

                return new InputMediaPhoto(InputFile.FromUri(url)) { Caption = caption };
            }
        }

        private static Process StartGalleryDL(string url)
        {
            return SystemHelpers.StartReadableProcess("gallery-dl", $"{url} -g");
        }

        private void SendSingleFilePost(PostData post)
        {
            var gif = post.URL.EndsWith(".gif");
            try
            {
                // todo fix bug: some gifs are sent as "fgsfds.gif.jpg" image document for no reason
                SendPicOrAnimation(InputFile.FromUri(post.URL));
            }
            catch
            {
                var meme = DownloadMeme(post, gif ? ".gif" : ".png");
                var process = meme.UseFFMpeg(Chat);
                var path = gif
                    ? process.CompressGIF().Result
                    : process.Compress   ().Result;
                
                using var stream = File.OpenRead(path);
                SendPicOrAnimation(InputFile.FromStream(stream, $"r-{post.Subreddit}.mp4"));
            }

            void SendPicOrAnimation(InputFile file)
            {
                if (gif) Bot.SendAnimaXD(Chat, file, post.Title);
                else     Bot.SendPhotoXD(Chat, file, post.Title);
            }
        }

        private PostData? GetPostOrBust(RedditQuery query)
        {
            try
            {
                return Reddit.PullPost(query, Chat);
            }
            catch
            {
                Bot.SendMessage(Chat, "💀");

                //                   He sends
                // awesome fucking evil blue flaming skull next to
                //  a keyboard with the "g" key being highlighted
                //                 to your chat
                // ⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄
                // ⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⢀⣶⣿⣿⣿⣿⣿⣿⣶⣆⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄
                // ⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⣸⣿⣿⠉⠉⠉⠄⠉⢹⣿⣦⡀⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄
                // ⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⢿⣿⣿⣁⠄⠄⠤⠤⡀⠻⣿⠃⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄
                // ⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠘⣿⣿⣿⡗⠖⡶⢾⣶⠊⡏⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄
                // ⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⢻⣿⣿⣅⣈⠂⠐⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄
                // ⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠘⢿⣾⣇⣂⣠⠄⠄⠄⠁⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄
                // ⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⢘⣿⣗⠒⠄⢨⠶⢁⣄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄
                // ⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠨⣿⣿⡿⠋⠁⣴⣿⣿⣷⣦⣄⡀⠄⠄⠄⠄⠄⠄⠄⠄
                // ⠄⠄⠄⠄⠄⠄⠄⠄⠄⢀⣠⣄⣶⣎⢱⢄⢀⣾⣿⣿⣿⣿⣿⣿⣿⣶⣦⣤⣄⠄⠄⠄⠄
                // ⠄⠄⠄⠄⠄⠄⠄⢠⣾⣿⣿⡞⢝⡟⠃⣠⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣯⣿⣿⣇⠄⠄⠄
                // ⠄⠄⠄⠄⠆⢄⠄⢛⡫⠝⢿⡥⠟⡃⣴⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣼⣭⣻⣿⣿⡀⠄⠄
                // ⠄⠄⠄⣴⣆⠄⢋⠄⠐⣡⣿⣆⣴⣼⣿⣿⣿⣿⣿⣿⣿⣿⠏⢈⣿⣿⣿⣿⣿⣿⣷⡄⠄
                // ⠄⠄⣼⣿⣷⠄⠉⠒⣪⣹⣟⣹⣿⣿⣿⣿⣿⣟⣿⣿⣿⡇⢀⣸⣿⣿⣿⢟⣽⣿⣿⣇⠄
                // WHOLESEOME 0 DESTRUCTION 100 QUAGMIRE TOILET 62
                return null;
            }
        }

        private static string DownloadMeme(PostData post, string extension)
        {
            var name = UniquePath(Dir_Pics, $@"{post.Fullname}{extension}");
            using var web = new WebClient();
            web.DownloadFile(post.URL, name);

            return name;
        }

        #endregion

        #region LOOKING FOR SUBREDDITS

        private static string SubredditList(string q, List<Subreddit> subs)
        {
            var sb = new StringBuilder(string.Format(SEARCH_HEADER, q, subs.Count, Ending(subs.Count)));
            foreach (var s in subs) sb.Append(string.Format(SUBS_LI, s.Name, FormatSubs(s.Subscribers ?? 0)));
            return sb.Append(string.Format(SEARCH_FOOTER, Bot.Me.FirstName)).ToString();
        }

        private const string SEARCH_HEADER = "По запросу <b>{0}</b> найдено <b>{1}</b> сообществ{2}:\n";
        private const string SUBS_LI       = "\n<code>{0}</code> - <i>{1}</i>";
        public  const string SEARCH_FOOTER = "\n\nБлагодарим за использование поисковика {0}";

        public  static string FormatSubs(int x, string bruh = "💀") => x switch
        {
            < 1000      =>  x + bruh,
            < 100_000   => (x / 1000D).ToString("0.#") + "k👌",
            < 1_000_000 =>  x / 1000      + "k👌",
            _           =>  x / 1_000_000 + "M 🤯"
        };
        private static string Ending(int x)
        {
            if (x is > 4 and < 21) return "";
            return (x % 10) switch { 1 => "o", 2 or 3 or 4 => "а", _ => ""};
        }

        #endregion
    }

    public class GetRedditLink : AsyncCommand
    {
        protected override async Task Run()
        {
            if (Message.ReplyToMessage is { } message)
            {
                Context = CommandContext.FromMessage(message);

                if (Text != null && await Recognize(Text) is { } post)
                {
                    Bot.SendMessage(Chat, $"<b><a href='{post.Permalink}'>r/{post.Subreddit}</a></b>");
                    Log($"{Title} >> LINK TO r/{post.Subreddit}");
                }
                else
                {
                    Bot.SendMessage(Chat, $"{I_FORGOR.PickAny()} {FAIL_EMOJI_1.PickAny()}");
                }
            }
            else Bot.SendMessage(Chat, string.Format(LINK_MANUAL, RedditTool.KEEP_POSTS));
        }

        private Task<PostData?> Recognize(string text)
        {
            return RedditTool.Queue.Enqueue(() => RedditTool.Instance.Recognize(text));
        }
    }
}