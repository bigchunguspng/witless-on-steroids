using PF_Bot.Core.Internet.Reddit;

namespace PF_Bot.Backrooms.Helpers;

public static class RedditHelpers
{
    private static readonly Regex
        _r_args = new(@"^(.*?)?(?:\s?([A-Za-z0-9_]+)\*)?(?:\s?-([hntrc][hdwmya]?))?$", RegexOptions.Compiled);

    /// Expected syntax: [search query] [subreddit*] [-options]
    /// <br/> Either search query, subreddit, or both should be provided.
    public static RedditQuery? ParseArgs_SearchOrScroll(string? args)
    {
        if (args == null) return null;

        var match = _r_args.Match(args);
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

    // todo you know what
    private static readonly Regex
        _rgx_arg = new(@"((.+)(?=\s[A-Za-z0-9_]+\*))|((.+)(?=\s-\S+))|(.+)", RegexOptions.Compiled | RegexOptions.ExplicitCapture),
        _rgx_sub = new(  "[A-Za-z0-9_]+",               RegexOptions.Compiled),
        _rgx_SUB = new(@"([A-Za-z0-9_]+)\*",            RegexOptions.Compiled),
        _rgx_ops = new(@"(?<=-)([hntrc][hdwmya]?)\S*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// Expected syntax: subreddit [-options]
    public static ScrollQuery? ParseArgs_ScrollQuery(string? args)
    {
        var sub = _rgx_sub.Match(args ?? "");
        if (sub.Failed()) return null;

        var subreddit = sub.Groups[0].Value;

        var options = args.GetOptions("ha");

        var sort = (Reddit_ScrollSort)options[0];
        var time = options.GetTime(TimeMatters(sort));

        return new ScrollQuery(subreddit, sort, time);
    }

    /// Expected syntax: search query [subreddit*] [-options]
    public static SearchQuery? ParseArgs_SearchQuery(string? args)
    {
        var arg = _rgx_arg.Match(args ?? "");
        if (arg.Failed()) return null;

        var text = arg.Groups[0].Value;

        var subreddit = _rgx_SUB.ExtractGroup(1, args ?? "", s => s);

        var options = args.GetOptions("ra");

        var sort = (Reddit_SearchSort)options[0];
        var time = options.GetTime(TimeMatters(sort));

        return new SearchQuery(subreddit, text, sort, time);
    }

    private static string GetOptions
        (this string? args, string defaluts)
        => _rgx_ops.ExtractGroup(0, args ?? "", s => s, defaluts);

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