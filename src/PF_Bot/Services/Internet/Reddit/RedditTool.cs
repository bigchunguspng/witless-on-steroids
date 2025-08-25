using Reddit;
using Reddit.Controllers;
using PF_Bot.Backrooms.Types.SerialQueue;

#pragma warning disable CS8524

namespace PF_Bot.Services.Internet.Reddit
{
    public class RedditTool
    {
        public const int POST_LIMIT = 32, KEEP_POSTS = 50;

        public static readonly RedditTool Instance = new();

        public static readonly SerialTaskQueue Queue = new();

        private readonly RedditClient client = new(Config.RedditAppID, Config.RedditToken, Config.RedditSecret);

        private readonly Regex _img = new(@"(\.png|\.jpg|\.jpeg|\.gif)$|(reddit\.com\/gallery\/)");

        private RedditTool()
        {
            Excluded = JsonIO.LoadData<Queue<string>>(File_RedditPosts);
            ConsoleUI.LoggedIntoReddit = true;
        }


        #region EXCLUSION

        private const int EXCLUDED_CAPACITY = 256;

        /// <summary> Posts that were sent to users recently, so they are no longer relevant. </summary>
        private readonly Queue<string> Excluded;

        private void Exclude(string fullname)
        {
            if (Excluded.Count == EXCLUDED_CAPACITY) Excluded.Dequeue();
            Excluded.Enqueue(fullname);
        }

        public void SaveExcluded()
        {
            JsonIO.SaveData(Excluded, File_RedditPosts);
        }

        #endregion


        #region LAST POSTS

        /// <summary> Used specifically for "/link" command. </summary>
        private readonly Queue<PostData> LastSent = new(KEEP_POSTS);

        private void Retain(PostData post)
        {
            if (LastSent.Count == KEEP_POSTS) LastSent.Dequeue();
            LastSent.Enqueue(post);
        }

        public PostData? Recognize(string title)
        {
            return LastSent.FirstOrDefault(x => x.Title == title);
        }

        #endregion


        #region LAST QUERIES

        /// <summary> Last queries by chat. </summary>
        private readonly Dictionary<long, RedditQuery> LastQueries = new();

        private void SetLastQuery(long chat, RedditQuery query)
        {
            LastQueries[chat] = query;
        }

        public RedditQuery GetLastOrRandomQuery(long chat)
        {
            return LastQueries.TryGetValue(chat, out var query) ? query : RandomSubredditQuery;
        }

        public ScrollQuery RandomSubredditQuery => new(RandomSubreddit);
        private string RandomSubreddit => subreddits[Random.Shared.Next(subreddits.Length)];

        private readonly string[] subreddits =
        [
            "comedynecrophilia", "okbuddybaka", "comedycemetery", "okbuddyretard",
            "dankmemes", "memes", "funnymemes", "doodoofard", "21stcenturyhumour",
            "breakingbadmemes", "minecraftmemes", "shitposting", "whenthe"
        ];

        #endregion


        #region PULLING POST

        private PostData _post = default!;

        /// <summary> Upcoming posts by query. </summary>
        private readonly Dictionary<RedditQuery, RedditQueryCache> Cache = new();

        private RedditQuery      ThisQuery = default!;
        private RedditQueryCache ThisQueryCache => Cache[ThisQuery];

        //

        public PostData PullPost(RedditQuery query, long chat)
        {
            ThisQuery = query;
            
            GetLatestRelevantPost();

            Exclude(_post.Fullname);
            SetLastQuery(chat, query);
            Retain(_post);
            return _post;
        }

        private void GetLatestRelevantPost()
        {
            do
            {
                UpdateCache();
                _post = ThisQueryCache.Posts.Dequeue();
            }
            while (ThisQueryCache is { HasEnoughPosts: true, Posts.Count: > 0 } && Excluded.Contains(_post.Fullname));
        }

        /// <summary>
        /// Refills <see cref="Cache"/> with new posts if needed.
        /// </summary>
        private void UpdateCache()
        {
            if (Cache.ContainsKey(ThisQuery)) // query has been used already
            {
                var posts = ThisQueryCache.Posts;
                if (ThisQueryCache.RefreshDate < DateTime.Now) // time to clear queue and load new posts
                {
                    ThisQueryCache.UpdateRefreshDate();
                    posts.Clear();
                    LogDebug("ScrollReddit (old posts)");
                    ScrollReddit();
                }
                else if (posts.Count == 1) // last post >> load next
                {
                    LogDebug("ScrollReddit (1 post)");
                    ScrollReddit(posts.Peek().Fullname);
                }
                else if (posts.Count == 0) // no posts in queue
                {
                    LogDebug("ScrollReddit (0 posts)");
                    ScrollReddit();
                }
            }
            else // no posts in queue (and no queue too)
            {
                Cache.Add(ThisQuery, new RedditQueryCache());

                LogDebug("ScrollReddit (new Q)");
                ScrollReddit();
            }
        }

        /// <summary>
        /// Gets the actual posts using <see cref="ThisQuery"/> and adds them to <see cref="ThisQueryCache"/>.
        /// </summary>
        private void ScrollReddit(string? after = null, int patience = 3)
        {
            var posts = ThisQuery.GetPosts(after);
            ThisQueryCache.HasEnoughPosts = posts.Count >= POST_LIMIT;

            var acceptable = GetOnlyImagePosts(posts);
            foreach (var post in acceptable) ThisQueryCache.Posts.Enqueue(post);
            
            Log($"Posts: {acceptable.Count}/{posts.Count}");

            // (there ARE posts, but NONE of them is an image)
            if (ThisQueryCache.Posts.Count == 0 && posts.Count > 0 && patience > 0 && ThisQueryCache.HasEnoughPosts)
            {
                ScrollReddit(posts[^1].Fullname, --patience);
            }
        }

        private List<PostData> GetOnlyImagePosts(ICollection<Post> posts)
        {
            var pinned = Math.Max(0, posts.Count - POST_LIMIT);
            return posts
                .Skip(pinned)
                .OfType<LinkPost>()
                .Where(post => _img.IsMatch(post.URL))
                .Select(post => new PostData(post))
                .ToList();
        }

        #endregion


        #region FETCHING COMMENTS

        public Task<List<string>> GetComments(RedditQuery query, int count = POST_LIMIT) => Task.Run(() =>
        {
            var texts = new List<string>(count);
            string? after = null;
            for (var i = 0; i < count; i += POST_LIMIT)
            {
                after = ScrollForComments(query, texts, after);
            }

            return texts;
        });

        private string ScrollForComments(RedditQuery query, List<string> list, string? after)
        {
            var posts = query.GetPosts(after);
            foreach (var post    in posts)
            foreach (var comment in post.Comments.GetTop())
            {
                GetCommentTexts(comment, list);
            }
            return posts[^1].Fullname;
        }

        /// <summary> Collects text from the comment and all its replies. </summary>
        private void GetCommentTexts(Comment comment, List<string> list)
        {
            if (comment.Body is not null) list.Add(comment.Body);
            foreach (var reply in comment.Replies) GetCommentTexts(reply, list);
        }

        #endregion


        #region GETTING POSTS

        public List<Post> GetPosts(ScrollQuery query, string? after = null)
        {
             
            var sub = client.Subreddit(query.Subreddit).Posts;
            return query.Sort switch
            {
                SortingMode.Hot           => sub.GetHot          (after: after, limit: POST_LIMIT),
                SortingMode.New           => sub.GetNew          (after: after, limit: POST_LIMIT),
                SortingMode.Top           => sub.GetTop          (after: after, limit: POST_LIMIT, t: query.Time),
                SortingMode.Rising        => sub.GetRising       (after: after, limit: POST_LIMIT),
                SortingMode.Controversial => sub.GetControversial(after: after, limit: POST_LIMIT, t: query.Time)
            };
        }

        public List<Post> SearchPosts(SearchQuery s, string? after = null)
        {
            if (s.Subreddit is null)
                return client.Search(s.Q, sort: s.Sort, t: s.Time, after: after, limit: POST_LIMIT);

            var subreddit = client.Subreddit(s.Subreddit);
            return  subreddit.Search(s.Q, sort: s.Sort, t: s.Time, after: after, limit: POST_LIMIT);
        }

        #endregion


        public int QueriesCached => Cache.Count;
        public int   PostsCached => Cache.Values.Sum(c => c.Posts.Count);

        public string DebugCache()
        {
            var rows = Cache
                .OrderByDescending(x => x.Value.Posts.Count)
                .Select(x => $"{x.Value.Posts.Count} - <code>{x.Key}</code>");

            return string.Join('\n', rows);
        }

        public List<Subreddit> FindSubreddits(string search)
        {
            return client.SearchSubreddits(search).Where(s => s.Subscribers > 0).ToList();
        }
    }
}