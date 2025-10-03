using System.Runtime.CompilerServices;
using PF_Bot.Core;
using PF_Tools.Backrooms.Types.SerialQueue;
using PF_Tools.Reddit;
using Reddit;
using Reddit.Controllers;

namespace PF_Bot.Features_Web.Reddit.Core;

public class RedditApp
{
    public  const int KEEP_POSTS = 256;
    private const int POST_LIMIT =  32, EXCLUDED_CAPACITY = 256;

    private readonly RedditTool _client = new
    (
        new RedditClient
        (
            Config.RedditAppID,
            Config.RedditToken,
            Config.RedditSecret
        ),
        POST_LIMIT
    );

    public static void Log(string text) => LogDebug($"Reddit > {text}");

    // OTHER WRAPPERS

    public Task<List<Subreddit>>  FindSubreddits(string search)
        => Task.Run(() => _client.FindSubreddits(search));

    public Task<List<string>>     GetComments(RedditQuery query)
        => Task.Run(() => _client.GetComments(query, POST_LIMIT));


    #region STATE

    /// Upcoming posts by query.
    private readonly Dictionary<RedditQuery, RedditQueryCache> Cache = new();

    // DEBUG INFO

    public int QueriesCached => Cache.Count;
    public int   PostsCached => Cache.Values.Sum(c => c.ImagePosts.Count);

    public IEnumerable<(int Count, RedditQuery Query)> DebugCache
        () => Cache
        .Select(x => (x.Value.ImagePosts.Count, Query: x.Key))
        .OrderByDescending(x => x.Count);

    // EXCLUDED POSTS (to avoid repeating)

    /// Recently sent posts, therefore no longer relevant.
    private readonly LimitedQueue<string> ExcludedPosts = LoadExcluded();

    public  void          Exclude (RedditPost post) => ExcludedPosts.Enqueue (post.Fullname);
    private bool PostIsNotExcluded(RedditPost post) => ExcludedPosts.Contains(post.Fullname).Janai();

    private static LimitedQueue<string> LoadExcluded
        () => new (EXCLUDED_CAPACITY, JsonIO.LoadData<List<string>>(File_RedditPosts));
    public                         void SaveExcluded
        () =>                         JsonIO.SaveData(ExcludedPosts, File_RedditPosts);

    // LAST POSTS (for /link)

    private readonly LimitedCache<string, RedditPost> LastPosts = new(KEEP_POSTS);

    /// Key - "chat_id-message_id" for single file posts and "media_group_id" for albums.
    [MethodImpl(MethodImplOptions.Synchronized)] public void     LastPosts_Remember
        (string key, RedditPost post) => LastPosts.Add(key, post);

    /// <inheritdoc cref="LastPosts_Remember"/>
    [MethodImpl(MethodImplOptions.Synchronized)] public RedditPost? LastPosts_TryGet
        (string key)                => LastPosts.Contains(key, out var post) ? post : null;

    // LAST QUERIES (for /ww)

    /// Last queries by chat.
    private readonly Dictionary<long, RedditQuery> LastQueries = new();

    public RedditQuery GetLastOrRandomQuery
        (long chat) => LastQueries.TryGetValue(chat, out var query)
        ? query
        : RandomSubredditQuery;

    public ScrollQuery RandomSubredditQuery => new(subreddits.PickAny());

    private readonly string[] subreddits =
    [
        "comedynecrophilia", "okbuddybaka", "comedycemetery", "okbuddyretard",
        "dankmemes", "memes", "funnymemes", "doodoofard", "21stcenturyhumour",
        "breakingbadmemes", "minecraftmemes", "shitposting", "whenthe",
    ];

    #endregion


    #region PULLING POST

    private readonly SerialTaskQueue _queue = new();

    public Task<RedditPost> PullPost(RedditQuery query, long chat) => _queue.Enqueue(() =>
    {
        LastQueries[chat] = query;

        while (true)
        {
            EnsureCacheIsNotEmpty(query);
            var cache = Cache[query];
            var post  = cache.ImagePosts.Dequeue(); // throws on empty!

            var relevant = cache.EndOfQueryResults || PostIsNotExcluded(post);
            if (relevant)
            {
                Log($"POST {post.Fullname} | Query cache: {cache.ImagePosts.Count,2} posts");
                return post;
            }
        }
    });

    // todo scroll after last post async ?
    /// Refills query cache with new posts if needed.
    /// Cache still can end up being empty if no posts were found.
    private void EnsureCacheIsNotEmpty(RedditQuery query)
    {
        if (Cache.TryGetValue(query, out var cache))
        {
            var posts = cache.ImagePosts;
            if (cache.IsOutdated)
            {
                cache.DelayRefreshDate();
                posts.Clear();
                Scroll_Logged("outdated posts");
            }
            else if (posts.Count == 1) Scroll_Logged("last post", posts.Peek().Fullname);
            else if (posts.Count == 0) Scroll_Logged("0 posts");
            //       posts.Count >  1  ?  return;
        }
        else
        {
            cache = new RedditQueryCache(POST_LIMIT);
            Cache.Add(query, cache);
            Scroll_Logged("0 posts, new Query");
        }

        void Scroll_Logged(string comment, string? after = null)
        {
            var where = after == null ? "start," : "after";
            Log($"Scroll | {where} {comment}");

            Scroll(query, cache, after);
        }
    }

    /// Gets posts by query and adds them to cache.
    /// Or doesn't if nothing's found.
    private void Scroll
    (
        RedditQuery query, RedditQueryCache cache,
        string? after = null, int patience = 3
    )
    {
        for (var i = 0; i < patience; i++)
        {
            if (ScrollOnce(query, cache, after)) return;
        }
    }

    /// Returns true if image posts OR last posts were reached.
    private bool ScrollOnce
        (RedditQuery query, RedditQueryCache cache, string? after = null)
    {
        var allPosts = _client.GetPosts(query, after);

        cache.EndOfQueryResults = allPosts.Count < POST_LIMIT;

        var imagePosts = TakeOnlyImagePosts(allPosts);
        foreach (var post in imagePosts) cache.ImagePosts.Enqueue(post);

        Log($"Scroll | Posts (img/all): {imagePosts.Count}/{allPosts.Count}");

        var success = imagePosts.Count >  0;
        var useless =   allPosts.Count == 0 || cache.EndOfQueryResults;

        return success || useless;
    }

    private readonly Regex
        _r_imagePost = new(@"\.(png|jpg|jpeg|gif)$|(reddit\.com\/gallery\/)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

    private List<RedditPost> TakeOnlyImagePosts
        (ICollection<Post> posts) => posts
        .Skip(Math.Max(0, posts.Count - POST_LIMIT)) // skip pinned posts
        .OfType<LinkPost>()                          // skip text posts
        .Where (post => _r_imagePost.IsMatch(post.URL))
        .Select(post => new RedditPost(post)).ToList();

    #endregion
}