using Reddit;
using Reddit.Controllers;
using static PF_Tools.Reddit.Reddit_ScrollSort;

namespace PF_Tools.Reddit;

public class RedditTool(RedditClient client, int POST_LIMIT)
{
    // POSTS

    public List<Post> GetPosts(RedditQuery query, string? after = null) => query switch
    {
        ScrollQuery scroll =>    GetPosts(scroll, after),
        SearchQuery search => SearchPosts(search, after),
        _ => throw new ArgumentException("Bro added a new reddit query..."),
    };

    public List<Post> GetPosts
        (ScrollQuery query, string? after = null) => query.Sort switch
    {
        Hot           => Subreddit(query).GetHot          (after: after, limit: POST_LIMIT),
        New           => Subreddit(query).GetNew          (after: after, limit: POST_LIMIT),
        Top           => Subreddit(query).GetTop          (after: after, limit: POST_LIMIT, t: query.Time.ToLower()),
        Rising        => Subreddit(query).GetRising       (after: after, limit: POST_LIMIT),
        Controversial => Subreddit(query).GetControversial(after: after, limit: POST_LIMIT, t: query.Time.ToLower()),
        _ => throw new ArgumentException("Bro added a new reddit sort option..."),
    };

    private SubredditPosts Subreddit
        (ScrollQuery query) => client.Subreddit(query.Subreddit).Posts;

    public List<Post> SearchPosts
        (SearchQuery s, string? after = null)
    {
        var sort = s.Sort.ToLower();
        var time = s.Time.ToLower();
        return s.Subreddit == null
            ? client
                .Search(s.Text, sort: sort, t: time, after: after, limit: POST_LIMIT)
            : client
                .Subreddit(s.Subreddit)
                .Search(s.Text, sort: sort, t: time, after: after, limit: POST_LIMIT);
    }


    // COMMENTS

    public List<string> GetComments(RedditQuery query, int count)
    {
        var texts = new List<string>();
        var after = (string?)null;
        for (var i = 0; i < count; i += POST_LIMIT)
        {
            var posts = GetPosts(query, after);

            foreach (var post    in posts)
            foreach (var comment in post.Comments.GetTop())
            {
                CollectCommentThread(comment, texts);
            }

            after = posts[^1].Fullname;
        }

        return texts;
    }

    private void CollectCommentThread(Comment comment, List<string> texts)
    {
        var text = comment.Body;
        if (text.IsNotNull_NorWhiteSpace()) texts.Add(text.Trim());

        foreach (var reply in comment.Replies)
        {
            CollectCommentThread(reply, texts);
        }
    }


    // SUBREDDITS

    public List<Subreddit> FindSubreddits
        (string search) => client
        .SearchSubreddits(search)
        .Where(s => s.Subscribers > 0).ToList();
}