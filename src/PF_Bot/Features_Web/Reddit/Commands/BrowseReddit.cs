using System.Net;
using System.Text;
using PF_Bot.Core;
using PF_Bot.Features_Aux.Packs.Core;
using PF_Bot.Features_Main.Edit.Core;
using PF_Bot.Features_Web.Reddit.Core;
using PF_Bot.Routing.Commands;
using PF_Tools.FFMpeg;
using PF_Tools.ProcessRunning;
using PF_Tools.Reddit;
using Reddit.Controllers;
using Telegram.Bot.Types;

#pragma warning disable SYSLIB0014

namespace PF_Bot.Features_Web.Reddit.Commands; // ReSharper disable InconsistentNaming

public class BrowseReddit : CommandHandlerAsync
{
    private static readonly RedditApp Reddit = App.Reddit;

    // input: /w {reply message}
    // input: /ww
    // input: /wss subreddit
    // input: /ws [subreddit [-ops]]
    // input: /w search query [subreddit*] [-ops]   (ops: -h/-n/-t/-c/-ta/...)
    protected override async Task Run()
    {
        if     (MessageRepliesToRequest())            await Run();
        else if (Options.Length == 0)      /* /w   */ await SearchPosts();
        else if (Options.StartsWith('s'))
            if  (Options.StartsWith("ss")) /* /wss */ await FindSubreddits();
            else                           /* /ws  */ await OpenSubreddit();
        else                               /* /ww  */ await Scroll();
    }

    private bool MessageRepliesToRequest()
    {
        var message = Message.ReplyToMessage;
        if (message == null) return false;

        var text = message.GetTextOrCaption();
        var success = text?.StartsWith("/w", StringComparison.OrdinalIgnoreCase) ?? false;
        if (success)
            Context = CommandContext.CreateOrdinary(message, Command);

        return success;
    }

    private async Task Scroll()
    {
        RedditApp.Log("/ww -> LAST QUERY");
        await SendPost(Reddit.GetLastOrRandomQuery(Chat));
    }

    private async Task FindSubreddits()
    {
        if (Args != null)
        {
            RedditApp.Log("/wss -> FIND SUBREDDITS");
            await SendSubredditList(Args);
        }
        else
        {
            Log($"{Title} >> REDDIT SUBS ?");
            SendManual(REDDIT_SUBS_MANUAL);
        }
    }

    private async Task OpenSubreddit()
    {
        if (RedditHelpers.ParseArgs_ScrollQuery(Args) is { } query)
        {
            RedditApp.Log("/ws -> SUBREDDIT");
            await SendPost(query);
        }
        else
        {
            Log($"{Title} >> REDDIT ?");
            SendManual(REDDIT_MANUAL);
        }
    }

    private async Task SearchPosts()
    {
        if (RedditHelpers.ParseArgs_SearchOrScroll(Args) is { } query)
        {
            RedditApp.Log(query is SearchQuery ? "/w -> SEARCH" : "/w -> SUBREDDIT");
            await SendPost(query);
        }
        else
        {
            RedditApp.Log("/w -> RANDOM SUBREDDIT");
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
            SetBadStatus();
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
        RedditApp.Log($"GALLERY | {post.URL}");
        var urls = await GalleryDl.Run($"{post.URL} -g");

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
            var input  =       DownloadMeme(post,  gif ? ".gif" : ".png");
            var output = await CompressMeme(input, gif);

            var filename = gif
                ? $"r-{post.Subreddit}.mp4"
                : null;

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

    private static async Task<FilePath> CompressMeme(FilePath input, bool gif)
    {
        var (output, probe, options) = await input.InitEditing("small", gif ? ".mp4" : ".jpg");

        options.MP4_EnsureSize_Valid_And_Fits(probe.GetVideoStream(), gif ? 1080 : 2560);

        _ = gif
            ? options
                .Options("-an")
                .FixVideo_Playback()
                .SetCRF(30)
            : options
                .Options("-qscale:v 5");

        await FFMpeg.Command(input, output, options).FFMpeg_Run();

        return output;
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