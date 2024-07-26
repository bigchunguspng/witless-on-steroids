using System;
using System.Collections.Generic;

namespace Witlesss.Services.Internet.Reddit;

#pragma warning disable CS8524
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