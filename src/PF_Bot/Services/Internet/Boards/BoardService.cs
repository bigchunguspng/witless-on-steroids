using System.Net;
using System.Web;
using HtmlAgilityPack;
using RestSharp;

namespace PF_Bot.Services.Internet.Boards
{
    public class BoardService
    {
        private const string BOARD_THREAD   = "//a[@class='replylink'][. = 'Reply']";
        private const string ARCHIVE_THREAD = "//a[@class='quotelink']";
        private const string SEARCH_THREAD  = "//span[@class='post_controls']/a[1]";

        private static readonly Regex _thread_post      = new(@"<blockquote.*?>(.*?)<\/blockquote>");
        private static readonly Regex _thread_post_desu = new(@"<div class=""text"">(.*?)<\/div>");
        private static readonly Regex _thread_subject      = new(@"<span class=""subject"">(.*?)<\/span>");
        private static readonly Regex _thread_subject_desu = new(@"<h2 class=""post_title"">(.*?)<\/h2>");
        private static readonly Regex _tags  = new("<.*?>");

        private readonly HtmlWeb _web = new();
        private readonly RestClient _rest = new();

        private Regex GetRegexForSubject
            (bool desu) => desu
            ? _thread_subject_desu
            : _thread_subject;

        private Regex GetRegexForPosts
            (bool desu) => desu
            ? _thread_post_desu
            : _thread_post;

        /// <inheritdoc cref="GetThreadDiscussion"/> Async version, starts immediately.
        public Task<List<string>> GetThreadDiscussionAsync
            (string url) => Task.Run(() => GetThreadDiscussion(url).ToList());

        /// <summary> Returns every single line of a thread. </summary>
        /// <param name="url">thread URL, like https://boards.4channel.org/a/thread/XXX</param>
        public IEnumerable<string> GetThreadDiscussion(string url)
        {
            var html = TryGetThreadHtml(url);
            if (html is null) yield break;

            var desu = url.Contains("desuarchive.org");
            var replyIndicator = desu ? " <span class=\"greentext\"><a" : "<a";

            var subject = GetRegexForSubject(desu).Matches(html)[desu ? 0 : ^1].Groups[1].Value;
            var subjectPending = !string.IsNullOrWhiteSpace(subject);

            var posts = GetRegexForPosts(desu).Matches(html).Select(x => x.Groups[1].Value);
            foreach (var post in posts)
            {
                var separator = desu ? "<br />" : "<br>";
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

        private string? TryGetThreadHtml(string threadURL)
        {
            var patience = 5;
            do
            {
                var response = _rest.Get(new RestRequest(threadURL, Method.GET));
                if (response.StatusCode == HttpStatusCode.OK) return response.Content;

                LogError($"Board Service -> TryGetThreadHtml -> {response.StatusCode}");
                Task.Delay(5000).Wait();
            }
            while (patience-- > 0);

            LogError("Board Service -> TryGetThreadHtml -> NO POSTS!?");
            return null;
        }

        /// <summary> Returns thread URLs from first 10 pages of the board. </summary>
        /// <param name="url">board URL, like https://boards.4channel.org/a/</param>
        public IEnumerable<string> GetAllActiveThreads(string url)
        {
            for (var i = 1; i <= 10; i++)
            {
                IEnumerable<string> hrefs;
                try
                {
                    var pageURL = i == 1 ? url : url + i;
                    hrefs = GetHrefs(pageURL, BOARD_THREAD);
                }
                catch // board can have < 10 pages
                {
                    yield break;
                }

                foreach (var href in hrefs) yield return href;
            }
        }

        /// <summary> Returns ARCHIVED thread URLs (local paths) from a board ARCHIVE page. </summary>
        /// <param name="url">board archive URL, like https://boards.4channel.org/a/archive</param>
        public IEnumerable<string> GetAllArchivedThreads
            (string url) => GetHrefs(url, ARCHIVE_THREAD);

        /// <summary> Returns thread URLs from the first page of https://desuarchive.org search. </summary>
        /// <param name="url"> use <see cref="GetDesuSearchURLText"/> to obtain.</param>
        public IEnumerable<string> GetSearchResults
            (string url) => GetHrefs(url, SEARCH_THREAD).Select(x => x.Remove(x.LastIndexOf('/')));

        /// <param name="place">board code or "_" to searh anywhere</param>
        /// <param name="query">search string</param>
        public string GetDesuSearchURLText
            (string place, string query) => $"https://desuarchive.org/{place}/search/text/{query}/type/op/";

        /// <inheritdoc cref="GetDesuSearchURLText"/>
        public string GetDesuSearchURLSubject
            (string place, string query) => $"https://desuarchive.org/{place}/search/subject/{query}/type/op/";

        private IEnumerable<string> GetHrefs
            (string url, string xpath)
            => _web.Load(url).DocumentNode.SelectNodes(xpath).Select(x => x.Attributes["href"].Value);


        // LISTING BOARDS

        private const string HOME_COLUMN = "//div[@class='column']";

        /// <param name="path">path to a saved home page file, since it's more reliable to hae it this way.</param>
        public List<BoardGroup> GetBoardList(string path)
        {
            var boards = new List<BoardGroup>();
            var group = new BoardGroup();

            var document = new HtmlDocument();
            document.Load(path);

            var columns = document.DocumentNode.SelectNodes(HOME_COLUMN);
            var nodes = columns.SelectMany(x => x.ChildNodes.Where(n => n.Name != "#text")).ToList();
            var last = nodes.Last();

            foreach (var node in nodes)
            {
                if (node.Name == "h3")
                {
                    var text = node.InnerText;
                    if (text == "(NSFW)")
                    {
                        group.IsNSFW = true;
                    }
                    else
                    {
                        group.Title = HttpUtility.HtmlDecode(text);
                    }
                }
                else if (node.Name == "ul")
                {
                    var items = node.ChildNodes.Where(x => x.Name == "li");
                    foreach (var item in items)
                    {
                        var name = HttpUtility.HtmlDecode(item.InnerText);
                        var href = item.ChildNodes[0].Attributes["href"].Value;

                        var key = new Uri(href).Segments[1].Trim('/');
                        group.Boards.Add(new BoardGroup.Board(name, key, href, false));
                    }
                    boards.Add(group);
                    if (node != last) group = new BoardGroup();
                }
            }

            return boards;
        }
    }
}