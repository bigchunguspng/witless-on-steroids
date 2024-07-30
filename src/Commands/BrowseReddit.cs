﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Reddit.Controllers;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Witlesss.Backrooms.Helpers;
using Witlesss.MediaTools;
using Witlesss.Services.Internet.Reddit;
using static Witlesss.XD.SortingMode;

#pragma warning disable CS8509
#pragma warning disable SYSLIB0014

namespace Witlesss.Commands // ReSharper disable InconsistentNaming
{
    public class BrowseReddit : SyncCommand
    {
        private readonly Regex _arg = new(@"((?:(?:.+)(?=\s[a-z0-9_]+\*))|(?:(?:.+)(?=\s-\S+))|(?:.+))");
        private readonly Regex _sub = new(@"([a-z0-9_]+)");
        private readonly Regex sub_ = new(@"([a-z0-9_]+)\*");
        private readonly Regex _ops = new(@"(?<=-)([hntrc][hdwmya]?)\S*$");
        private readonly Regex _wtf = new(@"^\/w[^\ss_@]");

        private static readonly RedditTool Reddit = RedditTool.Instance;

        // input: /w {reply message}
        // input: /ww
        // input: /wss subreddit
        // input: /ws [subreddit [-ops]]
        // input: /w search query [subreddit*] [-ops]   (ops: -h/-n/-t/-c/-ta/...)
        protected override void Run()
        {
            //var input = TextWithoutBotUsername;

            if (Message.ReplyToMessage is { Text: { } t } message && IsCommand(t, "/w"))
            {
                Context = CommandContext.FromMessage(message);
                Run(); // RECURSIVE
            }
            else if (_wtf.IsMatch(Command!))
            {
                Log("LAST QUERY");
                SendPost(Reddit.GetLastOrRandomQuery(Chat));
            }
            else if (Command!.StartsWith("/wss")) // subreddit
            {
                if (Args is not null)
                {
                    var subs = Reddit.FindSubreddits(Args);
                    var b = subs.Count > 0;

                    Bot.SendMessage(Chat, b ? SubredditList(Args, subs) : "<b>*пусто*</b>");
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

                    Log("SUBREDDIT");
                    SendPost(new ScrollQuery(subreddit, sort, time));
                }
                else
                {
                    Bot.SendMessage(Chat, REDDIT_MANUAL, preview: false);
                }
            }
            else // /w search query [subreddit*] [-ops]
            {
                var arg = _arg.Match(Args ?? "");
                if (arg.Success)
                {
                    var q = arg.Groups[1].Value;

                    var sub = sub_.Match(Args ?? "");
                    var s = sub.Success;
                    var subreddit = s ? sub.Groups[1].Value : null;

                    var options = GetOptions("ra");

                    var sort = Sorts  [options[0]];
                    var time = GetTime(options, TimeMatters(options[0]));

                    Log("SEARCH");
                    SendPost(new SearchQuery(subreddit, q, sort, time));
                }
                else
                {
                    Log("DEFAULT (RANDOM)");
                    SendPost(Reddit.RandomSubredditQuery);
                }
            }

            bool IsCommand(string a, string b) => a.ToLower().StartsWith(b);

            string GetOptions(string alt)
            {
                var ops = _ops.Match(Args ?? "");
                return ops.Success ? ops.Value : alt;
            }
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
        
        public  static bool TimeMatters(SortingMode s) => s is Top or Controversial;
        public  static bool TimeMatters(char        c) => c is not 'h' and not 'n';

        #region SENDING MEMES

        private void SendPost(RedditQuery query)
        {
            var post = GetPostOrBust(query);
            if (post == null) return;

            var a = post.URL.Contains("/gallery/");
            if (a)  SendGalleryPost(post);
            else SendSingleFilePost(post);

            if (ChatsDealer.WitlessExist(Chat, out var baka)) baka.Eat(post.Title);

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

                return new InputMediaPhoto(new InputMedia(url)) { Caption = caption };
            }
        }

        private static Process StartGalleryDL(string url)
        {
            return SystemHelpers.StartedReadableProcess("gallery-dl", $"{url} -g");
        }

        private void SendSingleFilePost(PostData post)
        {
            var gif = post.URL.EndsWith(".gif");
            try
            {
                // todo fix bug: some gifs are sent as "fgsfds.gif.jpg" image document for no reason
                SendPicOrAnimation(new InputOnlineFile(post.URL));
            }
            catch
            {
                var meme = DownloadMeme(post, gif ? ".gif" : ".png");
                var path = gif ? FFMpegXD.CompressGIF(meme).Result : FFMpegXD.Compress(meme).Result;
                
                using var stream = File.OpenRead(path);
                SendPicOrAnimation(new InputOnlineFile(stream, $"r-{post.Subreddit}.mp4"));
            }

            void SendPicOrAnimation(InputOnlineFile file)
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
            var name = UniquePath(Paths.Dir_Pics, $@"{post.Fullname}{extension}");
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

    public class GetRedditLink : SyncCommand
    {
        protected override void Run()
        {
            if (Message.ReplyToMessage is { } message)
            {
                Context = CommandContext.FromMessage(message);

                if (Text is not null && RedditTool.Instance.Recognize(Text) is { } post)
                {
                    Bot.SendMessage(Chat, $"<b><a href='{post.Permalink}'>r/{post.Subreddit}</a></b>", preview: false);
                }
                else
                {
                    Bot.SendMessage(Chat, $"{Responses.I_FORGOR.PickAny()} {Responses.FAIL_EMOJI_1.PickAny()}");
                }
            }
            else Bot.SendMessage(Chat,  string.Format(LINK_MANUAL, RedditTool.KEEP_POSTS));
        }
    }
}