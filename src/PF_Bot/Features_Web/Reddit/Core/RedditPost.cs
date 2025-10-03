using Reddit.Controllers;

namespace PF_Bot.Features_Web.Reddit.Core;

public class RedditPost(LinkPost post)
{
    public readonly string Fullname  = post.Fullname;
    public readonly string URL       = post.URL; // .png .jpg .gif
    public readonly string Title     = post.Title.Trim();
    public readonly string Subreddit = post.Subreddit;
    public readonly string Permalink = post.Permalink;
}