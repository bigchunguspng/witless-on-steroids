namespace PF_Bot.Core.Internet.Reddit;

public interface RedditQuery;

/// Uses <b>searchbar</b> on the main page or on a <b>subreddit</b>.
public record SearchQuery
(
    string? Subreddit,
    string Text,
    Reddit_SearchSort Sort,
    Reddit_TimeOption Time
)
    : RedditQuery
{
    public override string ToString()
    {
        var sub = Subreddit is null ? "" : $" {Subreddit}*";
        return $"SEARCH {Text}{sub} -{(char)Sort}{(char)Time}";
    }
}

/// Opens subreddit and <b>scrolls</b> for some posts.
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

public enum Reddit_TimeOption
{
    All           = 'a',
    Hour          = 'h',
    Day           = 'd',
    Week          = 'w',
    Month         = 'm',
    Year          = 'y',
};

public enum Reddit_SearchSort
{
    Hot           = 'h',
    New           = 'n',
    Top           = 't',
    Relevance     = 'r',
    Comments      = 'c',
};

public enum Reddit_ScrollSort
{
    Hot           = 'h',
    New           = 'n',
    Top           = 't',
    Rising        = 'r',
    Controversial = 'c',
}