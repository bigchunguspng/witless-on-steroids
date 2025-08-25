using System.Web;
using HtmlAgilityPack;
using RestSharp;

namespace PF_Bot.Services.Internet.Boards;

public class PlankService
{
    private const string BASE_URL = "https://2ch.hk";

    private static readonly Regex _thread_post    = new(@"<article id=""\S*?"" class=""post__message "">\s*(.*?)\s*<\/article>");
    private static readonly Regex _thread_subject = new(@"<span class=""post__title"">\s*(.*?)\s*<\/span>");
    private static readonly Regex _board_thread   = new(@"<span class=""post__detailpart desktop""><a href=""(.*?)"">Ответ");
    private static readonly Regex _search_thread  = new(@"<span class=""reflink"">\s*Тред: <a href=""(\S*?)""");
    private static readonly Regex _tags  = new("<.*?>");

    private readonly RestClient _rest = new();


    /// <inheritdoc cref="GetThreadDiscussion"/> Async version, starts immediately.
    public Task<List<string>> GetThreadDiscussionAsync
        (string url) => Task.Run(() => GetThreadDiscussion(url).ToList());

    /// <summary> Returns every single line of a thread. </summary>
    /// <param name="url">thread URL, like https://2ch.hk/a/res/XXX.html</param>
    public IEnumerable<string> GetThreadDiscussion(string url)
    {
        var response = _rest.Get(new RestRequest(url, Method.GET));
        var html = response.Content;
        
        var replyIndicator = "<a";

        var subject = _thread_subject.Match(html).Groups[1].Value;
        var subjectPending = !string.IsNullOrWhiteSpace(subject);

        var posts = _thread_post.Matches(html).Select(x => x.Groups[1].Value);
        foreach (var post in posts)
        {
            var separator = "<br>";
            var lines = post.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.StartsWith(replyIndicator)) continue; // skip things like ">>103424950 (OP)"

                var text = _tags.Replace(line, "");

                if (subjectPending) // add subject for the 1st line (if any)
                {
                    text = $"{subject}: {text}";
                    subjectPending = false;
                }

                yield return HttpUtility.HtmlDecode(text);
            }
        }
    }

    /// <summary> Returns URLs of first threads from a board. </summary>
    /// <param name="url">board URL, like https://2ch.hk/a/</param>
    public IEnumerable<string> GetSomeThreads(string url)
    {
        var response = _rest.Get(new RestRequest(url));
        var html = response.Content;

        return _board_thread.Matches(html).Select(x => x.Groups[1].Value).Select(x => $"{BASE_URL}{x}");
    }

    /// <summary> Returns unique URLs of threads found on a board by a text. </summary>
    public IEnumerable<string> GetSearchResults(string board, string text)
    {
        var request = new RestRequest($"{BASE_URL}/user/search", Method.POST) { AlwaysMultipartFormData = true };

        request.AddParameter("board", board);
        request.AddParameter("text", text);

        var response = _rest.Execute(request);
        var html = response.Content;

        return _search_thread.Matches(html)
            .Select(x => x.Groups[1].Value).Distinct()
            .Select(x => $"{BASE_URL}{x}").ToList();
    }


    // LISTING PLANKS

    private const string HOME_LI = "//ul[@class='boards__ul']/li";
    private static readonly Regex _menu_board_item = new(@"<a.*?href=""(.*?)"">(.*?)(<span.*?span>)?<\/a>"); 

    /// <param name="path">path to a saved home page file.</param>
    public List<BoardGroup> GetBoardList(string path)
    {
        var boards = new List<BoardGroup>();
        BoardGroup group = null!;

        var document = new HtmlDocument();
        document.Load(path);

        var items = document.DocumentNode.SelectNodes(HOME_LI).Select(x => x.InnerHtml);
        foreach (var item in items)
        {
            if (item.StartsWith("<a") || item.StartsWith("<font"))
            {
                var match = _menu_board_item.Match(item);
                var title = match.Groups[2].Value;
                var url   = match.Groups[1].Value;
                var nsfw  = match.Groups[3].Success;

                var uri = new Uri(url);
                if (uri.Host.StartsWith("2")      == false) continue; // tv, t.me
                if (uri.Segments[1].EndsWith('/') == false) continue; // api.yml

                var key = uri.Segments.Length > 2 ? null : uri.Segments[1].Trim('/');
                group.Boards.Add(new BoardGroup.Board(title, key, url, nsfw));
            }
            else
            {
                var nsfw = item.Contains("<");
                var title = nsfw ? item.Remove(item.IndexOf("<", StringComparison.Ordinal)).Trim() : item;

                group = new BoardGroup() { Title = title, IsNSFW = nsfw };
                boards.Add(group);
            }
        }

        return boards;
    }
}