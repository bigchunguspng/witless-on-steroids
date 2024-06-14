using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Reddit;
using Reddit.Controllers;
using static Witlesss.Config;

#pragma warning disable CS8524

namespace Witlesss.Services.Internet
{
    /// <summary> Used to store upcoming posts for a single <see cref="RedditQuery"/>. </summary>
    public class RedditQueryCache
    {
        /// <summary> DateTime by which the cache is relevant. </summary>
        public DateTime RefreshDate;

        /// <summary> True if posts in queue AIN'T the last ones for the query. </summary>
        public bool HasEnoughPosts;

        public readonly Queue<PostData> Posts = new(RedditTool.POST_LIMIT);

        public RedditQueryCache() => UpdateRefreshDate();

        public void UpdateRefreshDate() => RefreshDate = DateTime.Now + TimeSpan.FromHours(2);
    }

    public class RedditTool
    {
        public const int POST_LIMIT = 32, KEEP_POSTS = 50;

        public static readonly RedditTool Instance = new();

        private readonly RedditClient client = new(RedditAppID, RedditToken);

        private readonly Regex _img = new(@"(\.png|\.jpg|\.gif)$|(reddit\.com\/gallery\/)");

        private RedditTool()
        {
            Excluded = ExcludedIO.LoadData();
            ConsoleUI.LoggedIntoReddit = true;
        }


        #region EXCLUSION

        private const int EXCLUDED_CAPACITY = 256;

        /// <summary> Posts that were sent to users recently, so they are no longer relevant. </summary>
        private readonly        Queue<string>  Excluded;
        private readonly FileIO<Queue<string>> ExcludedIO = new("reddit-posts.json");

        private void Exclude(string fullname)
        {
            if (Excluded.Count == EXCLUDED_CAPACITY) Excluded.Dequeue();
            Excluded.Enqueue(fullname);
        }

        public void SaveExcluded()
        {
            ExcludedIO.SaveData(Excluded);
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

        public ScQuery RandomSubredditQuery => new(RandomSubreddit);
        private string RandomSubreddit => subreddits[Random.Shared.Next(subreddits.Length)];

        private readonly string[] subreddits = 
        {
            "comedynecrophilia", "okbuddybaka", "comedycemetery", "okbuddyretard",
            "dankmemes", "memes", "funnymemes", "doodoofard", "21stcenturyhumour",
            "breakingbadmemes", "minecraftmemes", "shitposting", "whenthe"
        };

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
                    Log("ScrollReddit (old posts)", ConsoleColor.DarkYellow);
                    ScrollReddit();
                }
                else if (posts.Count == 1) // last post >> load next
                {
                    Log("ScrollReddit (1 post)", ConsoleColor.DarkYellow);
                    ScrollReddit(posts.Peek().Fullname);
                }
                else if (posts.Count == 0) // no posts in queue
                {
                    Log("ScrollReddit (0 posts)", ConsoleColor.DarkYellow);
                    ScrollReddit();
                }
            }
            else // no posts in queue (and no queue too)
            {
                Cache.Add(ThisQuery, new RedditQueryCache());

                Log("ScrollReddit (new Q)", ConsoleColor.DarkYellow);
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

        public List<Post> GetPosts(ScQuery query, string? after = null)
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

        public List<Post> SearchPosts(SsQuery s, string? after = null)
        {
            var subreddit = client.Subreddit(s.Subreddit);
            return subreddit.Search(s.Q, sort: s.Sort, t: s.Time, after: after, limit: POST_LIMIT);
        }

        public List<Post> SearchPosts(SrQuery s, string? after = null)
        {
            return    client.Search(s.Q, sort: s.Sort, t: s.Time, after: after, limit: POST_LIMIT);
        }

        #endregion


        public int QueriesCached => Cache.Count;
        public int   PostsCached => Cache.Values.Sum(c => c.Posts.Count);

        public List<Subreddit> FindSubreddits(string search)
        {
            return client.SearchSubreddits(search).Where(s => s.Subscribers > 0).ToList();
        }
    }


    public interface RedditQuery { List<Post> GetPosts(string? after = null); }

    /// <summary> Uses <b>searchbar</b> on a main page. </summary>
    public record SrQuery(string Q, string Sort, string Time) : RedditQuery
    {
        public List<Post> GetPosts(string? after = null) => RedditTool.Instance.SearchPosts(this, after);
    }

    /// <summary> Uses <b>searchbar</b> on a <b>subreddit</b>. </summary>
    public record SsQuery(string Subreddit, string Q, string Sort, string Time) : RedditQuery
    {
        public List<Post> GetPosts(string? after = null) => RedditTool.Instance.SearchPosts(this, after);
    }

    /// <summary> Opens subreddit and <b>scrolls</b> for some posts. </summary>
    public record ScQuery(string Subreddit, SortingMode Sort = SortingMode.Hot, string Time = "all") : RedditQuery
    {
        public List<Post> GetPosts(string? after = null) => RedditTool.Instance.GetPosts(this, after);
    }

    public class PostData(LinkPost post)
    {
        public string Fullname  { get; } = post.Fullname;
        public string URL       { get; } = post.URL; // .png .jpg .gif
        public string Title     { get; } = post.Title;
        public string Subreddit { get; } = post.Subreddit;

        private readonly string _permalink = post.Permalink;
        public string Permalink => $"https://www.reddit.com{_permalink}";
    }
}