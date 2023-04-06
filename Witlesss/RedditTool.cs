﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Reddit;
using Reddit.Controllers;

#pragma warning disable CS8524

namespace Witlesss
{
    public class RedditQueryCache
    {
        // the number of posts that the tool scrolls before losing patience
        // has to be cleared sometimes
        //private const int EXCLUDED_CAPACITY = 128;
        
        public DateTime RefreshDate;
        public bool HasEnoughPosts;
        public readonly Queue<PostData> Posts = new(RedditTool.POST_LIMIT);
        //public Queue<string> Excluded = new(EXCLUDED_CAPACITY);

        public RedditQueryCache()
        {
            RefreshDate = GetRefreshDate();
        }

        public void UpdateRefreshDate() => RefreshDate = GetRefreshDate();

        private static DateTime GetRefreshDate() => DateTime.Now + TimeSpan.FromHours(2);
    }
    public class RedditTool
    {
        private const int EXCLUDED_CAPACITY = 256;
        public  const int POST_LIMIT = 32, KEEP_POSTS = 50;

        public static readonly RedditTool Instance = new();

        private PostData _post;

        private readonly RedditClient client = new(RedditAppID, RedditToken);
        private readonly string[] subreddits = 
        {
            "comedynecrophilia", "okbuddybaka", "comedycemetery", "okbuddyretard",
            "dankmemes", "memes", "funnymemes", "doodoofard", "21stcenturyhumour",
            "breakingbadmemes", "minecraftmemes", "shitposting", "whenthe"
        };

        private readonly Regex _img = new(@"(.png|.jpg|.gif)$");

        private readonly Dictionary<long, RedditQuery> LastQueries = new();

        private readonly Dictionary<RedditQuery, RedditQueryCache> Cache = new();

        private RedditQuery Qr;
        private RedditQueryCache QrCache => Cache[Qr];
        
        private readonly Queue<PostData> LastSent = new(KEEP_POSTS);

        private readonly        Queue<string>  Excluded;
        private readonly FileIO<Queue<string>> ExcludedIO = new("reddit-posts.json");
        private readonly Counter counter = new() { Interval = 16 };

        private RedditTool() => Excluded = ExcludedIO.LoadData();


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

        public RedditQuery LastQueryOrRandom(long chat) => LastQueries.ContainsKey(chat) ? LastQueries[chat] : RandomSubQuery;
        public ScQuery RandomSubQuery => new(RandomSub);
        private string RandomSub => subreddits[Extension.Random.Next(subreddits.Length)];

        private void SetLastQuery(long chat, RedditQuery query)
        {
            if (LastQueries.ContainsKey(chat)) LastQueries[chat] = query;
            else
                LastQueries.Add(chat, query);
        }
        
        public PostData PullPost(RedditQuery query, long chat) // returns post, register post
        {
            Qr = query;
            
            GetUnwatchedPost();

            Exclude (_post.Fullname);
            Remember(_post);
            SetLastQuery(chat, query);
            return _post;
        }

        private void GetUnwatchedPost()
        {
            do
            {
                CheckCache();
                _post = QrCache.Posts.Dequeue();
            }
            while (QrCache.HasEnoughPosts && QrCache.Posts.Count > 0 && Excluded.Contains(_post.Fullname));
        }

        private void CheckCache()
        {
            if (Cache.ContainsKey(Qr))
            {
                var posts = QrCache.Posts;
                if (QrCache.RefreshDate < DateTime.Now) // time to clear queue and load new posts
                {
                    QrCache.UpdateRefreshDate();
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
                Cache.Add(Qr, new RedditQueryCache());

                Log("ScrollReddit (new Q)", ConsoleColor.DarkYellow);
                ScrollReddit();
            }
        }

        private void ScrollReddit(string after = null, int patience = 3)
        {
            var queue = QrCache.Posts;
            var posts = Qr.GetPosts(after);
            foreach (var post in FilterImagePosts(posts)) queue.Enqueue(post);

            // there are posts, but none of them are image
            if (queue.Count == 0 && posts.Count > 0 && patience > 0 && QrCache.HasEnoughPosts)
            {
                ScrollReddit(posts[^1].Fullname, --patience);
            }
        }
        
        public List<Post> GetPosts(ScQuery query, string after = null)
        {
            var sub = client.Subreddit(query.Subreddit).Posts;
            return Count(query.Sort switch
            {
                SortingMode.Hot           => sub.GetHot          (after: after, limit: POST_LIMIT),
                SortingMode.New           => sub.GetNew          (after: after, limit: POST_LIMIT),
                SortingMode.Top           => sub.GetTop          (after: after, limit: POST_LIMIT, t: query.Time),
                SortingMode.Rising        => sub.GetRising       (after: after, limit: POST_LIMIT),
                SortingMode.Controversial => sub.GetControversial(after: after, limit: POST_LIMIT, t: query.Time)
            });
        }

        public List<Post> SearchPosts(SsQuery s, string after = null)
        {
            var sub = client.Subreddit(s.Subreddit);
            return Count(sub.Search   (s.Q, sort: s.Sort, t: s.Time, after: after, limit: POST_LIMIT));
        }

        public List<Post> SearchPosts(SrQuery s, string after = null)
        {
            return Count(client.Search(s.Q, sort: s.Sort, t: s.Time, after: after, limit: POST_LIMIT));
        }

        private List<PostData> FilterImagePosts(ICollection<Post> posts)
        {
            var pinned = Math.Max(0, posts.Count - POST_LIMIT);
            return posts.Skip(pinned).Where(IsValidPost).Select(p => new PostData(p as LinkPost)).ToList();

            bool IsValidPost(Post p) => p is LinkPost post && _img.IsMatch(post.URL);
        }
        
        public void LogInfo() // todo delete... this is a debug method
        {
            foreach (var que in Cache) Log($"Q: {que.Key.ToString()} C: {que.Value.Posts.Count}", ConsoleColor.Cyan);
        }

        public List<Subreddit> FindSubreddits(string search)
        {
            return client.SearchSubreddits(search).Where(s => s.Subscribers > 0).ToList();
        }

        private List<Post> Count(List<Post> posts)
        {
            Log("Posts: " + posts.Count);
            QrCache.HasEnoughPosts = posts.Count >= POST_LIMIT;
            return posts;
        }

        // new & hot no time
        // Query: q string, sort string, time string
        /*public void Fart()
        {
            var qwe = client.Subreddit("okbuddybaka").Search();
            var axb = client.Search(q: "osaka azumanga", sort: "new");
            var axd = client.Subreddit("okbuddybaka").Search("osaka", sort: "new");
            //var asd = client.SearchSubredditNames("osaka azumanga");
            foreach (var post in axd)
            {
                Console.WriteLine(post.Title);
            }
        }*/
    }

    public interface RedditQuery { List<Post> GetPosts(string after = null); }

    /// <summary> Uses searchbar on a main page </summary>
    public record SrQuery(string Q, string Sort, string Time) : RedditQuery
    {
        public override string ToString() => $"{Q} ({Sort} {Time})";
        
        public List<Post> GetPosts(string after = null) => RedditTool.Instance.SearchPosts(this, after);
    }

    /// <summary> Uses searchbar on a subreddit </summary>
    public record SsQuery(string Subreddit, string Q, string Sort, string Time) : RedditQuery
    {
        public override string ToString() => $"r/{Subreddit} - {Q} ({Sort} {Time})";
        
        public List<Post> GetPosts(string after = null) => RedditTool.Instance.SearchPosts(this, after);
    }

    /// <summary> Opens subreddit and scrolls for some posts </summary>
    public record ScQuery(string Subreddit, SortingMode Sort = SortingMode.Hot, string Time = "all") : RedditQuery
    {
        public List<Post> GetPosts(string after = null) => RedditTool.Instance.GetPosts(this, after);
        
        public override string ToString() => $"r/{Subreddit} ({Sort} {Time})";
    }

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