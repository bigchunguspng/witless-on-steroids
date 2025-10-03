using PF_Tools.Reddit;

namespace PF_Bot.Features_Web.Reddit.Core;

/// Used to store upcoming posts for a single <see cref="RedditQuery"/>.
public class RedditQueryCache(int capacity)
{
    public readonly Queue<RedditPost> ImagePosts = new(capacity);

    /// Whether posts in <see cref="ImagePosts"/> are the last ones for the query.
    public bool EndOfQueryResults;

    public bool IsOutdated => _bestBy < DateTime.Now;

    private DateTime                   _bestBy = GetDate_2H_Later();
    public  void DelayRefreshDate() => _bestBy = GetDate_2H_Later();

    private static DateTime GetDate_2H_Later() => DateTime.Now + TimeSpan.FromHours(2);
}