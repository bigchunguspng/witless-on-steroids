using PF_Tools.Backrooms.Types.SerialQueue;
using Reddit;
using Reddit.Controllers;

#pragma warning disable CS8524

namespace PF_Bot.Core.Internet.Reddit
{
    public class RedditTool
    {
        public const int POST_LIMIT = 32, KEEP_POSTS = 50;

        public static readonly RedditTool Instance = new();

        public static readonly SerialTaskQueue Queue = new();

        private readonly RedditClient client = new(Config.RedditAppID, Config.RedditToken, Config.RedditSecret);

        private RedditTool()
        {
            ExcludedPosts = JsonIO.LoadData<Queue<string>>(File_RedditPosts);
            App.LoggedIntoReddit = true;
        }


        #region EXCLUSION

        private const int EXCLUDED_CAPACITY = 256;

        /// Posts that were sent to users recently, so they are no longer relevant.
        private readonly Queue<string> ExcludedPosts;

        private void Exclude(PostData post)
        {
            if (ExcludedPosts.Count == EXCLUDED_CAPACITY) ExcludedPosts.Dequeue();
            ExcludedPosts.Enqueue(post.Fullname);
        }

        private bool PostIsNotExcluded(PostData post)
        {
            return ExcludedPosts.Contains(post.Fullname).Janai();
        }

        public void SaveExcluded()
        {
            JsonIO.SaveData(ExcludedPosts, File_RedditPosts);
        }

        #endregion


        #region LAST POSTS

        /// Used specifically for "/link" command.
        private readonly Queue<PostData> LastSentPosts = new(KEEP_POSTS);

        private void Remember(PostData post)
        {
            if (LastSentPosts.Count == KEEP_POSTS) LastSentPosts.Dequeue();
            LastSentPosts.Enqueue(post);
        }

        // todo - use cache <(chat, message), post data>
        public PostData? Recognize(string title)
        {
            return LastSentPosts.FirstOrDefault(x => x.Title == title);
        }

        #endregion


        #region LAST QUERIES

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

        /// Upcoming posts by query.
        private readonly Dictionary<RedditQuery, RedditQueryCache> Cache = new();

        //

        public PostData PullPost(RedditQuery query, long chat)
        {
            var post = GetRelevantPost(query);

            Exclude (post);
            Remember(post);

            LastQueries[chat] = query;

            return post;
        }

        private PostData GetRelevantPost(RedditQuery query)
        {
            while (true)
            {
                EnsureCacheIsNotEmpty(query);
                var cache = Cache[query];
                var post  = cache.ImagePosts.Dequeue(); // throws on empty!

                var relevant = cache.EndOfQueryResults || PostIsNotExcluded(post);
                if (relevant)
                {
                    LogDebug($"Reddit > POST {post.Fullname} | Query cache: {cache.ImagePosts.Count,2} posts");
                    return post;
                }
            }
        }

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
                cache = new RedditQueryCache();
                Cache.Add(query, cache);
                Scroll_Logged("0 posts, new Query");
            }

            void Scroll_Logged(string comment, string? after = null)
            {
                var where = after == null ? "start," : "after";
                LogDebug($"Reddit > Scroll | {where} {comment}");

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
            var allPosts = GetPosts(query, after);

            cache.EndOfQueryResults = allPosts.Count < POST_LIMIT;

            var imagePosts = TakeOnlyImagePosts(allPosts);
            foreach (var post in imagePosts) cache.ImagePosts.Enqueue(post);

            LogDebug($"Reddit > Scroll | Posts (img/all): {imagePosts.Count}/{allPosts.Count}");

            var success = imagePosts.Count >  0;
            var useless =   allPosts.Count == 0 || cache.EndOfQueryResults;

            return success || useless;
        }

        private readonly Regex
            _r_imagePost = new(@"\.(png|jpg|jpeg|gif)$|(reddit\.com\/gallery\/)", RegexOptions.Compiled | RegexOptions.ExplicitCapture);

        private List<PostData> TakeOnlyImagePosts
            (ICollection<Post> posts) => posts
            .Skip(Math.Max(0, posts.Count - POST_LIMIT)) // skip pinned posts
            .OfType<LinkPost>()                          // skip text posts
            .Where (post => _r_imagePost.IsMatch(post.URL))
            .Select(post => new PostData(post)).ToList();

        #endregion


        #region FETCHING COMMENTS

        public Task<List<string>> GetComments(RedditQuery query, int count = POST_LIMIT) => Task.Run(() =>
        {
            var texts = new List<string>();
            var after = (string?)null;
            for (var i = 0; i < count; i += POST_LIMIT)
            {
                var posts = GetPosts(query, after);

                foreach (var post    in posts)
                foreach (var comment in post.Comments.GetTop())
                {
                    CollectCommentThread(comment, texts);
                }

                after = posts[^1].Fullname;
            }

            return texts;
        });

        private void CollectCommentThread(Comment comment, List<string> texts)
        {
            if (comment.Body != null) texts.Add(comment.Body);

            foreach (var reply in comment.Replies)
            {
                CollectCommentThread(reply, texts);
            }
        }

        #endregion


        #region GETTING POSTS

        private List<Post> GetPosts(RedditQuery query, string? after = null)
            =>    query is ScrollQuery scroll ?    GetPosts(scroll, after)
                : query is SearchQuery search ? SearchPosts(search, after)
                : throw new ArgumentException("Bro added a new reddit query...");

        private List<Post> GetPosts(ScrollQuery query, string? after = null)
        {
            var sub = client.Subreddit(query.Subreddit).Posts;
            return query.Sort switch
            {
                Reddit_ScrollSort.Hot           => sub.GetHot          (after: after, limit: POST_LIMIT),
                Reddit_ScrollSort.New           => sub.GetNew          (after: after, limit: POST_LIMIT),
                Reddit_ScrollSort.Top           => sub.GetTop          (after: after, limit: POST_LIMIT, t: query.Time.ToLower()),
                Reddit_ScrollSort.Rising        => sub.GetRising       (after: after, limit: POST_LIMIT),
                Reddit_ScrollSort.Controversial => sub.GetControversial(after: after, limit: POST_LIMIT, t: query.Time.ToLower()),
            };
        }

        private List<Post> SearchPosts(SearchQuery s, string? after = null)
        {
            return s.Subreddit == null
                ? client
                    .Search(s.Text, sort: s.Sort.ToLower(), t: s.Time.ToLower(), after: after, limit: POST_LIMIT)
                : client
                    .Subreddit(s.Subreddit)
                    .Search(s.Text, sort: s.Sort.ToLower(), t: s.Time.ToLower(), after: after, limit: POST_LIMIT);
        }

        #endregion


        // DEBUG INFO

        public int QueriesCached => Cache.Count;
        public int   PostsCached => Cache.Values.Sum(c => c.ImagePosts.Count);

        public IEnumerable<(int Count, RedditQuery Query)> DebugCache
            () => Cache
            .OrderByDescending(x => x.Value.ImagePosts.Count)
            .Select(x => (x.Value.ImagePosts.Count, x.Key));

        // FIND SUBREDDITS

        public List<Subreddit> FindSubreddits
            (string search) => client
            .SearchSubreddits(search)
            .Where(s => s.Subscribers > 0)
            .ToList();
    }
}