using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Reddit;
using Reddit.Controllers;

#pragma warning disable CS8524

namespace Witlesss
{
    public class RedditTool
    {
        private const int EXCLUDED_CAPACITY = 256, POST_LIMIT = 32;
        public  const int KEEP_POSTS = 50;

        private bool HasEnoughPosts; // todo this by query (and move all query related stuff to a separate class)
        private PostData _post;

        private readonly RedditClient client = new(Config.RedditAppID, Config.RedditToken);
        private readonly string[] subreddits = 
        {
            "comedynecrophilia", "okbuddybaka", "comedycemetery", "okbuddyretard",
            "dankmemes", "memes", "funnymemes", "doodoofard", "21stcenturyhumour",
            "breakingbadmemes", "minecraftmemes", "shitposting", "whenthe"
        };

        private readonly Regex _img = new(@"(.png|.jpg|.gif)$");

        private readonly Dictionary<long, SrQuery> LastQueries = new();

        private readonly Dictionary<SrQuery, Queue<PostData>> Posts = new();
        private readonly Dictionary<SrQuery, DateTime> RefreshDates = new();

        private readonly Queue<PostData> LastSent = new(KEEP_POSTS);

        private readonly        Queue<string>  Excluded;
        private readonly FileIO<Queue<string>> ExcludedIO = new("reddit-posts.json");
        private readonly Counter counter = new() { Interval = 16 };

        public RedditTool() => Excluded = ExcludedIO.LoadData();


        private void Exclude(string fullname)
        {
            if (Excluded.Count == EXCLUDED_CAPACITY) Excluded.Dequeue();
            Excluded.Enqueue(fullname);
            
            counter.Count();
            if (counter.Ready()) SaveExcluded();
        }
        public void SaveExcluded() => ExcludedIO.SaveData(Excluded);

        public PostData Recall(string title) => LastSent.FirstOrDefault(x => x.Title == title);
        private void Remember(PostData post)
        {
            if (LastSent.Count == KEEP_POSTS) LastSent.Dequeue();
            LastSent.Enqueue(post);
        }

        public SrQuery LastQueryOrRandom(long chat) => LastQueries.ContainsKey(chat) ? LastQueries[chat] : RandomSubQuery;
        public SrQuery RandomSubQuery => new(RandomSub);
        private string RandomSub => subreddits[Extension.Random.Next(subreddits.Length)];

        private void SetLastQuery(long chat, SrQuery query)
        {
            if (LastQueries.ContainsKey(chat)) LastQueries[chat] = query;
            else
                LastQueries.Add(chat, query);
        }
        
        public PostData PullPost(SrQuery query, long chat)
        {
            GetUnwatchedPost(query);

            Exclude (_post.Fullname);
            Remember(_post);
            SetLastQuery(chat, query);
            return _post;
        }

        private void GetUnwatchedPost(SrQuery query)
        {
            do
            {
                CheckCache(query);
                _post = Posts[query].Dequeue();
            }
            while (HasEnoughPosts && Posts[query].Count > 0 && Excluded.Contains(_post.Fullname));
        }

        private void CheckCache(SrQuery query)
        {
            if (Posts.ContainsKey(query))
            {
                var posts = Posts[query];
                if (RefreshDates[query] < DateTime.Now) // time to clear queue and load new posts
                {
                    RefreshDates[query] = GetRefreshDate();
                    posts.Clear();
                    Log("ScrollReddit (old posts)", ConsoleColor.DarkYellow);
                    ScrollReddit(query);
                }
                else if (posts.Count == 1) // last post >> load next
                {
                    Log("ScrollReddit (1 post)", ConsoleColor.DarkYellow);
                    ScrollReddit(query, posts.Peek().Fullname);
                }
                else if (posts.Count == 0) // no posts in queue
                {
                    Log("ScrollReddit (0 posts)", ConsoleColor.DarkYellow);
                    ScrollReddit(query);
                }
            }
            else // no posts in queue (and no queue too)
            {
                Posts.Add(query, new Queue<PostData>(POST_LIMIT));
                RefreshDates.Add(query, GetRefreshDate());

                Log("ScrollReddit (new Q)", ConsoleColor.DarkYellow);
                ScrollReddit(query);
            }
        }

        private void ScrollReddit(SrQuery query, string after = null, int patience = 3)
        {
            var queue = Posts[query];
            var posts = GetPosts(query, after);
            foreach (var post in FilterImagePosts(posts)) queue.Enqueue(post);

            // there are posts, but none of them are image
            if (queue.Count == 0 && posts.Count > 0 && patience > 0 && HasEnoughPosts)
            {
                ScrollReddit(query, posts[^1].Fullname, --patience);
            }
        }
        
        private List<Post> GetPosts(SrQuery query, string after = null)
        {
            var sub = client.Subreddit(query.Subreddit).Posts;
            var posts = query.Sort switch
            {
                SortingMode.Hot           => sub.GetHot          (after: after, limit: POST_LIMIT),
                SortingMode.New           => sub.GetNew          (after: after, limit: POST_LIMIT),
                SortingMode.Top           => sub.GetTop          (after: after, limit: POST_LIMIT, t: query.Time),
                SortingMode.Rising        => sub.GetRising       (after: after, limit: POST_LIMIT),
                SortingMode.Controversial => sub.GetControversial(after: after, limit: POST_LIMIT, t: query.Time)
            };
            Log("GetPosts: " + posts.Count);
            HasEnoughPosts = posts.Count >= POST_LIMIT;
            return posts;
        }

        private List<PostData> FilterImagePosts(ICollection<Post> posts)
        {
            var pinned = Math.Max(0, posts.Count - POST_LIMIT);
            return posts.Skip(pinned).Where(IsValidPost).Select(p => new PostData(p as LinkPost)).ToList();

            bool IsValidPost(Post p) => p is LinkPost post && _img.IsMatch(post.URL);
        }
        
        private static DateTime GetRefreshDate() => DateTime.Now + TimeSpan.FromHours(2);
        
        public void LogInfo() // todo delete... this is a debug method
        {
            foreach (var que in Posts) Log($"Q: {que.Key.Subreddit} {que.Key.Sort} {que.Key.Time} C: {que.Value.Count}", ConsoleColor.Cyan);
        }
    }

    public record SrQuery(string Subreddit, SortingMode Sort = SortingMode.Hot, string Time = "all");

    public class PostData
    {
        private readonly string _permalink;
        
        public string Fullname  { get; }
        public string URL       { get; } // .png .jpg .gif
        public string Title     { get; }
        public string Subreddit { get; }
        public string Permalink { get => $"https://www.reddit.com{_permalink}"; private init => _permalink = value; }

        public PostData(LinkPost post)
        {
            Fullname  = post.Fullname;
            URL       = post.URL;
            Title     = post.Title;
            Subreddit = post.Subreddit;
            Permalink = post.Permalink;
        }
    }
}