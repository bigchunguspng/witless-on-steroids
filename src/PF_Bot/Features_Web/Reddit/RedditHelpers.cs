using PF_Tools.Reddit;
using static System.Text.RegularExpressions.RegexOptions;

namespace PF_Bot.Features_Web.Reddit;

public static class RedditHelpers
{
    private static readonly Regex
        _r_args_search = new(@"^(.*?)?(?:\s?([A-Za-z0-9_]+)\*)?(?:\s?-([hntrc][hdwmya]?))?$", Compiled),
        _r_args_scroll = new(@"            ^([A-Za-z0-9_]+)?   (?:\s?-([hntrc][hdwmya]?))?$", Compiled | IgnorePatternWhitespace);

    /// Expected syntax: [search query] [subreddit*] [-options]
    /// <br/> Either search query, subreddit, or both should be provided.
    public static RedditQuery? ParseArgs_SearchOrScroll(string? args)
    {
        if (args == null) return null;

        var match = _r_args_search.Match(args);
        if (match.Failed()) return null;

        var query     = match.ExtractGroup(1, s => s);
        var subreddit = match.ExtractGroup(2, s => s);
        var options   = match.ExtractGroup(3, s => s, query != null ? "ra" : "ha");

        if (query != null)
        {
            var sort = (Reddit_SearchSort)options[0];
            var time = options.GetTime(TimeMatters(sort));

            return new SearchQuery(subreddit, query, sort, time);
        }

        if (subreddit != null)
        {
            var sort = (Reddit_ScrollSort)options[0];
            var time = options.GetTime(TimeMatters(sort));

            return new ScrollQuery(subreddit, sort, time);
        }

        return null;
    }

    /// Expected syntax: subreddit [-options]
    public static ScrollQuery? ParseArgs_ScrollQuery(string? args)
    {
        if (args == null) return null;

        var match = _r_args_scroll.Match(args);
        if (match.Failed()) return null;

        var subreddit = match.ExtractGroup(1, s => s);
        if (subreddit == null) return null;

        var options   = match.ExtractGroup(2, s => s, "ha");

        var sort = (Reddit_ScrollSort)options[0];
        var time = options.GetTime(TimeMatters(sort));

        return new ScrollQuery(subreddit, sort, time);
    }

    private static Reddit_TimeOption GetTime
        (this string options, bool timeMatters)
        => options.Length > 1 && timeMatters
            ? (Reddit_TimeOption)options[1]
            :  Reddit_TimeOption.All;

    private static bool TimeMatters
        (Reddit_SearchSort s) =>
        s != Reddit_SearchSort.Hot
     && s != Reddit_SearchSort.New;

    private static bool TimeMatters
        (Reddit_ScrollSort s) =>
        s != Reddit_ScrollSort.Hot
     && s != Reddit_ScrollSort.New
     && s != Reddit_ScrollSort.Rising;
}