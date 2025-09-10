namespace PF_Bot.Tools_Legacy.RedditSearch;

#pragma warning disable CS8524
/// Used to store upcoming posts for a single <see cref="RedditQuery"/>.
public class RedditQueryCache
{
    /// DateTime by which the cache is relevant.
    public DateTime RefreshDate;

    /// True if posts in queue AIN'T the last ones for the query.
    public bool HasEnoughPosts;

    public readonly Queue<PostData> Posts = new(RedditTool.POST_LIMIT);

    public RedditQueryCache() => UpdateRefreshDate();

    public void UpdateRefreshDate() => RefreshDate = DateTime.Now + TimeSpan.FromHours(2);
}