using PF_Bot.Core;
using PF_Bot.Features_Aux.Packs.Core;
using PF_Bot.Features_Web.Reddit.Core;
using PF_Bot.Routing.Commands;
using PF_Tools.ProcessRunning;
using PF_Tools.Reddit;
using Telegram.Bot.Types;

namespace PF_Bot.Features_Web.Reddit.Commands; // ReSharper disable InconsistentNaming

public class BrowseReddit : CommandHandlerAsync
{
    private static readonly RedditApp Reddit = App.Reddit;

    // input: /w {reply message}
    // input: /ww
    // input: /ws [subreddit [-ops]]
    // input: /w search query [subreddit*] [-ops]   (ops: -h/-n/-t/-c/-ta/...)
    protected override async Task Run()
    {
        if     (MessageRepliesToRequest())          await Run();
        else if (Options.Length == 0)     /* /w  */ await SearchPosts();
        else if (Options.StartsWith('s')) /* /ws */ await OpenSubreddit();
        else                              /* /ww */ await Scroll();
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

    // SENDING MEMES

    private async Task SendPost(RedditQuery query)
    {
        var post = await Reddit.PullPost(query, Chat).OrDefault_OnException();
        if (post != null)
        {
            var sendPost = post.URL.Contains("/gallery/")
                ? SendGalleryBatched(post)
                : SendSingleFilePost(post);

            await sendPost;
            Reddit.Exclude(post);

            if (PackManager.BakaIsLoaded(Chat, out var baka))
                baka.Eat(post.Title);

            Log($"{Title} >> r/{post.Subreddit} <- {query}");
        }
        else
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
        }
    }

    // GALLERY

    private async Task SendGalleryBatched(RedditPost post)
    {
        RedditApp.Log($"GALLERY | {post.URL}");
        var urls = await GalleryDl.Run($"{post.URL} -g");

        var origin = Origin;
        for (var i = 0; i < 5; i++)
        {
            var batch = urls
                .Skip(10 * i).Take(10)
                .ToArray();

            if (batch.Length == 0) break;

            var caption = i == 0 ? post.Title : null;

            var messages = await SendGalleryChunk_AsAlbum(origin, batch, caption);
            if (messages?.Length > 0)
            {
                var message = messages[0];
                origin = (Chat, message.Id);

                Reddit.LastPosts_Remember(message.MediaGroupId!, post);
            }
            else break;
        }
    }

    private async Task<Message[]?> SendGalleryChunk_AsAlbum
        (MessageOrigin origin, Uri[] urls, string? caption)
    {
        try // Send from URLs
        {
            var album = urls
                .Select(url => new InputMediaPhoto(InputFile.FromUri(url)))
                .ToList();

            return await SendAlbum(album);
        }
        catch // Some files are too big -> compress
        {
            RedditApp.Log("DOWNLOAD GALLERY");

            var inputs = urls     // Download
                .Select(url => url.ToString())
                .Select(async (url) => await RedditHelpers.DownloadMeme(url))
                .ToArray();
            await Task.WhenAll(inputs);

            var outputs = inputs  // Compress
                .Select(task => task.Result)
                .Select(async (input) => await RedditHelpers.CompressMeme(input))
                .ToArray();
            await Task.WhenAll(outputs);

            var streams = outputs // Open
                .Select(task => task.Result)
                .Select(file => File.OpenRead(file))
                .ToArray();

            var album = streams
                .Select(stream => new InputMediaPhoto(InputFile.FromStream(stream)))
                .ToList();

            var messages = await SendAlbum(album);
            streams.ForEach(x => x.DisposeAsync());
            return messages;
        }

        async Task<Message[]> SendAlbum(List<InputMediaPhoto> album)
        {
            if (caption != null) album[0].Caption = caption;

            return await Bot.SendAlbum_OrThrow(origin, album);
        }
    }

    // SINGLE POST

    private async Task SendSingleFilePost(RedditPost post)
    {
        var gif = post.URL.EndsWith(".gif");
        try
        {
            await SendPicOrAnimation(InputFile.FromUri(post.URL));
        }
        catch
        {
            RedditApp.Log($"DOWNLOAD | {post.URL}");

            var input  = await RedditHelpers.DownloadMeme(post.URL);
            var output = await RedditHelpers.CompressMeme(input, gif);

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
}