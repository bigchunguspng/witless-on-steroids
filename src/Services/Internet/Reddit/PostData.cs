using Reddit.Controllers;

namespace Witlesss.Services.Internet.Reddit;

public class PostData(LinkPost post)
{
    public string Fullname  { get; } = post.Fullname;
    public string URL       { get; } = post.URL; // .png .jpg .gif
    public string Title     { get; } = post.Title.Trim();
    public string Subreddit { get; } = post.Subreddit;

    private readonly string _permalink = post.Permalink;
    public string Permalink => $"https://www.reddit.com{_permalink}";
}