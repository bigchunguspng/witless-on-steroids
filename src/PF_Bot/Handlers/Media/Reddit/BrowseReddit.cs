using System.Net;
using System.Text;
using PF_Bot.Backrooms.Helpers;
using PF_Bot.Core;
using PF_Bot.Core.Editing;
using PF_Bot.Core.Internet.Reddit;
using PF_Bot.Core.Text;
using PF_Bot.Routing.Commands;
using PF_Tools.FFMpeg;
using PF_Tools.ProcessRunning;
using PF_Tools.Reddit;
using Reddit.Controllers;
using Telegram.Bot.Types;

#pragma warning disable CS8509
#pragma warning disable SYSLIB0014

namespace PF_Bot.Handlers.Media.Reddit // ReSharper disable InconsistentNaming
{
    public class BrowseReddit : AsyncCommand
    {
        private static readonly Regex
            _rgx_wtf = new(@"^\/w[^s_@]", RegexOptions.Compiled);

        private static readonly RedditApp Reddit = App.Reddit;

        // input: /w {reply message}
        // input: /ww
        // input: /wss subreddit
        // input: /ws [subreddit [-ops]]
        // input: /w search query [subreddit*] [-ops]   (ops: -h/-n/-t/-c/-ta/...)
        protected override async Task Run()
        {
            if        (MessageRepliesToRequest()) await Run();
            else if (_rgx_wtf.IsMatch (Command!)) await Scroll();
            else if (Command!.StartsWith("/wss")) await LookForSubreddits();
            else if (Command!.StartsWith("/ws" )) await OpenSubreddit();
            else /*                       /w   */ await RedditSearch_OrOpenRandom();
        }

        private bool MessageRepliesToRequest()
        {
            var message = Message.ReplyToMessage;
            if (message == null) return false;

            var text = message.GetTextOrCaption();
            var success = text?.StartsWith("/w", StringComparison.OrdinalIgnoreCase) ?? false;
            if (success)
                Context = CommandContext.FromMessage(message);

            return success;
        }

        private async Task Scroll()
        {
            LogDebug("Reddit > LAST QUERY");
            await SendPost(Reddit.GetLastOrRandomQuery(Chat));
        }

        private async Task LookForSubreddits()
        {
            if (Args != null)
            {
                LogDebug("Reddit > FIND SUBS");
                await SendSubredditList(Args);
            }
            else
            {
                Log($"{Title} >> REDDIT SUBS ?");
                Bot.SendMessage(Origin, REDDIT_SUBS_MANUAL);
            }
        }

        private async Task OpenSubreddit()
        {
            if (RedditHelpers.ParseArgs_ScrollQuery(Args) is { } query)
            {
                LogDebug("Reddit > SUBREDDIT");
                await SendPost(query);
            }
            else
            {
                Log($"{Title} >> REDDIT ?");
                Bot.SendMessage(Origin, REDDIT_MANUAL);
            }
        }

        private async Task RedditSearch_OrOpenRandom()
        {
            if (RedditHelpers.ParseArgs_SearchQuery(Args) is { } query)
            {
                LogDebug("Reddit > SEARCH");
                await SendPost(query);
            }
            else
            {
                LogDebug("Reddit > DEFAULT (RANDOM)");
                await SendPost(Reddit.RandomSubredditQuery);
            }
        }

        #region SENDING MEMES

        private async Task SendPost(RedditQuery query)
        {
            var post = await GetPostOrBust(query);
            if (post == null) return;

            var sendPost = post.URL.Contains("/gallery/")
                ? SendGalleryPost   (post)
                : SendSingleFilePost(post);

            await sendPost;
            Reddit.Exclude(post);

            if (PackManager.BakaIsLoaded(Chat, out var baka)) baka.Eat(post.Title);

            Log($"{Title} >> r/{post.Subreddit} <- {query}");
        }

        private async Task<RedditPost?> GetPostOrBust(RedditQuery query)
        {
            try
            {
                return await Reddit.PullPost(query, Chat);
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

        private async Task SendGalleryPost(RedditPost post)
        {
            LogDebug($"Reddit > GALLERY | {post.URL}");
            var urls = await Run_GalleryDl($"{post.URL} -g");

            var origin = Origin;
            for (var i = 0; i < 5; i++)
            {
                var album = urls
                    .Skip(10 * i).Take(10)
                    .Select(url => new InputMediaPhoto(InputFile.FromUri(url)))
                    .ToList();

                if (album.Count == 0) break;

                if (i == 0) album[0].Caption = post.Title;

                var messages = Bot.SendAlbum(origin, album);
                if (messages?.Length > 0)
                {
                    var message = messages[0];
                    origin = (Chat, message.Id);

                    Reddit.LastPosts_Remember(message.MediaGroupId!, post);
                }
                else break;
            }
        }

        private static async Task<List<Uri>> Run_GalleryDl
            (string arguments, string directory = "")
        {
            var urls = new List<Uri>();
            var startedProcess =
                ProcessStarter.StartProcess_WithOutputHandler
                    (GALLERY_DL, arguments, directory, Output_Handler);

            await startedProcess.Process.WaitForExitAsync();

            var result = new ProcessResult(arguments, startedProcess);
            if (result.Failure) 
                throw new ProcessException(GALLERY_DL, result);

            return urls;

            void Output_Handler(string? data, StringBuilder output)
            {
                output.Append(data).Append('\n');

                if (data.IsNull_OrWhiteSpace()) return;

                Print(data);

                if (Uri.TryCreate(data, UriKind.Absolute, out var url))
                {
                    urls.Add(url);
                }
            }
        }

        private async Task SendSingleFilePost(RedditPost post)
        {
            var gif = post.URL.EndsWith(".gif");
            try
            {
                // todo fix bug: some gifs are sent as "fgsfds.gif.jpg" image document for no reason
                await SendPicOrAnimation(InputFile.FromUri(post.URL));
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

                var filename = gif ? $"r-{post.Subreddit}.mp4" : null;
                await using var stream = File.OpenRead(output);
                await SendPicOrAnimation(InputFile.FromStream(stream, filename));
            }

            async Task SendPicOrAnimation(InputFile file)
            {
                var message = gif
                    ? await Bot.SendAnima_OrThrow(Origin, file, post.Title)
                    : await Bot.SendPhoto_OrThrow(Origin, file, post.Title);

                Reddit.LastPosts_Remember(message.Format_ChatMessage(), post);
            }
        }

        private static FilePath DownloadMeme(RedditPost post, string extension)
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

        #endregion
    }
}