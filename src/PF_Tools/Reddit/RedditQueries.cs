namespace PF_Tools.Reddit;

public interface RedditQuery;

/// Posts from search page, either global or limited to a subreddit.
public record SearchQuery
(
    string? Subreddit,
    string Text,
    Reddit_SearchSort Sort = Reddit_SearchSort.Relevance,
    Reddit_TimeOption Time = Reddit_TimeOption.All
)
    : RedditQuery
{
    public override string ToString()
    {
        var sub = Subreddit is null ? "" : $" {Subreddit}*";
        return $"SEARCH {Text}{sub} -{(char)Sort}{(char)Time}";
    }
}

/// Posts from subreddit page.
public record ScrollQuery
(
    string Subreddit,
    Reddit_ScrollSort Sort = Reddit_ScrollSort.Hot,
    Reddit_TimeOption Time = Reddit_TimeOption.All
)
    : RedditQuery
{
    public override string ToString()
    {
        return $"SCROLL {Subreddit} -{(char)Sort}{(char)Time}";
    }
}