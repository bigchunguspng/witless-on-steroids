using System.Net;
using System.Text;
using PF_Bot.Core.Chats;
using PF_Bot.Core.Editing;
using PF_Bot.Core.Internet.Reddit;
using PF_Bot.Routing.Commands;
using PF_Tools.FFMpeg;
using PF_Tools.ProcessRunning;
using Reddit.Controllers;
using Telegram.Bot.Types;

#pragma warning disable CS8509
#pragma warning disable SYSLIB0014

namespace PF_Bot.Handlers.Media.Reddit // ReSharper disable InconsistentNaming
{
    public class BrowseReddit : AsyncCommand
    {
        private static readonly Regex
            _rgx_arg = new(@"((?:(?:.+)(?=\s[A-Za-z0-9_]+\*))|(?:(?:.+)(?=\s-\S+))|(?:.+))", RegexOptions.Compiled),
            _rgx_sub = new(@"([A-Za-z0-9_]+)",              RegexOptions.Compiled),
            _rgx_SUB = new(@"([A-Za-z0-9_]+)\*",            RegexOptions.Compiled),
            _rgx_ops = new(@"(?<=-)([hntrc][hdwmya]?)\S*$", RegexOptions.Compiled | RegexOptions.IgnoreCase),
            _rgx_wtf = new(@"^\/w[^\ss_@]",                 RegexOptions.Compiled);

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
            else if (_rgx_wtf.IsMatch(Command!))
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

                    Bot.SendMessage(Origin, b ? SubredditList(Args, subs) : "<b>*пусто*</b>");
                    Log($"{Title} >> FIND {subs.Count} SUBS >> {Args}");
                }
                else
                {
                    Bot.SendMessage(Origin, REDDIT_SUBS_MANUAL);
                }
            }
            else if (Command!.StartsWith("/ws")) // [subreddit [-ops]]
            {
                var sub = _rgx_sub.Match(Args ?? "");
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
                    Bot.SendMessage(Origin, REDDIT_MANUAL);
                }
            }
            else // /w search query [subreddit*] [-ops]
            {
                var arg = _rgx_arg.Match(Args ?? "");
                if (arg.Success)
                {
                    var q = arg.Groups[1].Value;

                    var subreddit = _rgx_SUB.ExtractGroup(1, Args ?? "", s => s);

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

            string GetOptions(string alt) => _rgx_ops.ExtractGroup(0, Args ?? "", s => s, alt)!;
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

        private async Task SendPost(RedditQuery query)
        {
            var post = GetPostOrBust(query);
            if (post == null) return;

            var a = post.URL.Contains("/gallery/");
            if (a)  await SendGalleryPost(post);
            else       SendSingleFilePost(post);

            if (ChatManager.BakaIsLoaded(Chat, out var baka)) baka.Eat(post.Title);

            Log($"{Title} >> r/{post.Subreddit} (Q:{Reddit.QueriesCached} P:{Reddit.PostsCached})");
        }

        private PostData? GetPostOrBust(RedditQuery query)
        {
            try
            {
                return Reddit.PullPost(query, Chat);
            }
            catch
            {
                Bot.SendMessage(Origin, "💀");

                //                   He sends
                // awesome fucking evil blue flaming skull next to
                //  a keyboard with the "g" key being highlighted
                //                 to your chat
                // ⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄
                // ⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⢀⣶⣿⣿⣿⣿⣿⣿⣶⣆⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄
                // ⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⣸⣿⣿⠉⠉⠉⠄⠉⢹⣿⣦⡀⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄
                // ⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⢿⣿⣿⣁⠄⠄⠤⠤⡀⠻⣿⠃⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄
                // ⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠘⣿⣿⣿⡗⠖⡶⢾⣶⠊⡏⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄
                // ⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⢻⣿⣿⣅⣈⠂⠐⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄
                // ⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠘⢿⣾⣇⣂⣠⠄⠄⠄⠁⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄
                // ⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⢘⣿⣗⠒⠄⢨⠶⢁⣄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄
                // ⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠨⣿⣿⡿⠋⠁⣴⣿⣿⣷⣦⣄⡀⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄
                // ⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⢀⣠⣄⣶⣎⢱⢄⢀⣾⣿⣿⣿⣿⣿⣿⣿⣶⣦⣤⣄⠄⠄⠄⠄⠄⠄
                // ⠄⠄⠄⠄⠄⠄⠄⠄⠄⠄⢠⣾⣿⣿⡞⢝⡟⠃⣠⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣯⣿⣿⣇⠄⠄⠄⠄⠄
                // ⠄⠄⠄⠄⠄⠄⠄⠆⢄⠄⢛⡫⠝⢿⡥⠟⡃⣴⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣼⣭⣻⣿⣿⡀⠄⠄⠄⠄
                // ⠄⠄⠄⠄⠄⠄⣴⣆⠄⢋⠄⠐⣡⣿⣆⣴⣼⣿⣿⣿⣿⣿⣿⣿⣿⠏⢈⣿⣿⣿⣿⣿⣿⣷⡄⠄⠄⠄
                // ⠄⠄⠄⠄⠄⣼⣿⣷⠄⠉⠒⣪⣹⣟⣹⣿⣿⣿⣿⣿⣟⣿⣿⣿⡇⢀⣸⣿⣿⣿⢟⣽⣿⣿⣇⠄⠄⠄
                // WHOLESEOME 0 DESTRUCTION 100 QUAGMIRE TOILET 62
                return null;
            }
        }

        private async Task SendGalleryPost(PostData post)
        {
            var processResult = await ProcessRunner.Run(GALLERY_DL, $"{post.URL} -g");
            if (processResult.Failure) throw new ProcessException(GALLERY_DL, processResult);

            var urls = new List<string>();
            using var reader = new StringReader(processResult.Output.ToString());
            for (var i = 0; i < 50; i++)
            {
                var line = await reader.ReadLineAsync();
                if (line is null) break;

                urls.Add(line);
            }

            var origin = Origin;
            for (var i = 0; i < 5; i++)
            {
                var album = urls.Skip(10 * i).Take(10).Select(url => new InputMediaPhoto(InputFile.FromUri(url))).ToList();
                if (album.Count == 0) break;
                if (i == 0) album[0].Caption = post.Title;

                var messages = Bot.SendAlbum(origin, album);
                if (messages?.Length > 0)    origin = (Chat, messages[0].Id);
                else break;
            }
        }

        private async Task SendSingleFilePost(PostData post)
        {
            var gif = post.URL.EndsWith(".gif");
            try
            {
                // todo fix bug: some gifs are sent as "fgsfds.gif.jpg" image document for no reason
                SendPicOrAnimation(InputFile.FromUri(post.URL));
            }
            catch
            {
                var input = DownloadMeme(post, gif ? ".gif" : ".png");

                var (output, probe, options) = await input.InitEditing("small", gif ? ".mp4" : ".jpg");

                options.MP4_EnsureSize_Valid_And_Fits(probe.GetVideoStream(), gif ? 1080 : 2560);

                if (gif)
                {
                    options
                        .Options("-an")
                        .FixVideo_Playback()
                        .SetCRF(30);
                }
                else
                    options
                        .Options("-qscale:v 5");

                await FFMpeg.Command(input, output, options).FFMpeg_Run();

                await using var stream = File.OpenRead(output);
                SendPicOrAnimation(InputFile.FromStream(stream, $"r-{post.Subreddit}.mp4"));
            }

            void SendPicOrAnimation(InputFile file)
            {
                if (gif) Bot.SendAnimaXD(Origin, file, post.Title);
                else     Bot.SendPhotoXD(Origin, file, post.Title);
            }
        }

        private static FilePath DownloadMeme(PostData post, string extension)
        {
            var name = Dir_RedditMemes
                .EnsureDirectoryExist()
                .Combine($"{post.Fullname}{extension}")
                .MakeUnique();
            using var web = new WebClient();
            web.DownloadFile(post.URL, name);

            return name;
        }

        #endregion

        #region LOOKING FOR SUBREDDITS

        private static string SubredditList(string q, List<Subreddit> subs)
        {
            var sb = new StringBuilder(string.Format(SEARCH_HEADER, q, subs.Count, subs.Count.ED("о", "а", "")));
            foreach (var s in subs) sb.Append(string.Format(SUBS_LI, s.Name, (s.Subscribers ?? 0).Format_bruh_1k_100k_1M()));
            return sb.Append(string.Format(SEARCH_FOOTER, Bot.Me.FirstName)).ToString();
        }

        private const string SEARCH_HEADER = "По запросу <b>{0}</b> найдено <b>{1}</b> сообществ{2}:\n";
        private const string SUBS_LI       = "\n<code>{0}</code> - <i>{1}</i>";
        public  const string SEARCH_FOOTER = "\n\nБлагодарим за использование поисковика {0}";

        #endregion
    }
}