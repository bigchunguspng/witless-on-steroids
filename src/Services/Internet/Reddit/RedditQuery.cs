using Reddit.Controllers;

namespace Witlesss.Services.Internet.Reddit;

public interface RedditQuery { List<Post> GetPosts(string? after = null); }

/// <summary> Uses <b>searchbar</b> on the main page or on a <b>subreddit</b>. </summary>
public record SearchQuery(string? Subreddit, string Q, string Sort, string Time) : RedditQuery
{
    public List<Post> GetPosts(string? after = null) => RedditTool.Instance.SearchPosts(this, after);
}

/// <summary> Opens subreddit and <b>scrolls</b> for some posts. </summary>
public record ScrollQuery(string Subreddit, SortingMode Sort = SortingMode.Hot, string Time = "all") : RedditQuery
{
    public List<Post> GetPosts(string? after = null) => RedditTool.Instance.GetPosts(this, after);
}

public enum SortingMode
{
    Hot           = 'h',
    New           = 'n',
    Top           = 't',
    Rising        = 'r',
    Controversial = 'c'
}