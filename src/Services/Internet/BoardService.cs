using System.Web;
using HtmlAgilityPack;

namespace Witlesss.Services.Internet
{
    public class BoardService
    {
        private const string BOARD_THREAD   = "//a[@class='replylink'][. = 'Reply']";
        private const string ARCHIVE_THREAD = "//a[@class='quotelink']";
        private const string THREAD_POST = "//blockquote";
        private const string THREAD_POST_DESU = "//div[@class='text']";
        private const string THREAD_SUBJECT = "//span[@class='subject']";
        private const string THREAD_SUBJECT_DESU = "//h2[@class='post_title']";
        private const string HOME_COLUMN     = "//div[@class='column']";

        private readonly Regex _tags  = new("<.*?>");
        private readonly HtmlWeb _web = new();

        private HtmlNode GetDocument
            (string url) => _web.Load(url).DocumentNode;

        private string GetXPathToSubject
            (bool desu) => desu
            ? THREAD_SUBJECT_DESU
            : THREAD_SUBJECT;

        private string GetXPathToPosts
            (bool desu) => desu
            ? THREAD_POST_DESU
            : THREAD_POST;

        /// <inheritdoc cref="GetThreadDiscussion"/> Async version, starts immediately.
        public Task<List<string>> GetThreadDiscussionAsync
            (string url) => Task.Run(() => GetThreadDiscussion(url).ToList());

        /// <summary> Returns every single line of a thread. </summary>
        /// <param name="url">thread URL, like https://boards.4channel.org/a/thread/XXX</param>
        public IEnumerable<string> GetThreadDiscussion(string url)
        {
            var document = GetDocument(url);

            var desu = url.Contains("desuarchive.org");
            var replyIndicator = desu ? " <span class=\"greentext\"><a" : "<a";

            var subject = document.SelectNodes(GetXPathToSubject(desu))[desu ? 0 : ^1].InnerHtml;
            var subjectPending = !string.IsNullOrWhiteSpace(subject);

            var posts = document.SelectNodes(GetXPathToPosts(desu));
            foreach (var post in posts)
            {
                var lines = post.InnerHtml.Split("<br>", StringSplitOptions.RemoveEmptyEntries);
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

        /// <summary> Returns thread URLs from first 10 pages of the board. </summary>
        /// <param name="url">board URL, like https://boards.4channel.org/a/</param>
        public IEnumerable<string> GetAllActiveThreads(string url)
        {
            for (var i = 1; i <= 15; i++)
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

        /// <summary> Returns ARCHIVED thread URLs from a board ARCHIVE page. </summary>
        /// <param name="url">board archive URL, like https://boards.4channel.org/a/archive</param>
        public IEnumerable<string> GetAllArchivedThreads
            (string url) => GetHrefs(url, ARCHIVE_THREAD);

        private IEnumerable<string> GetHrefs
            (string url, string xpath)
            => GetDocument(url).SelectNodes(xpath).Select(x => x.Attributes["href"].Value);


        // LISTING BOARDS

        /// <param name="path">path to a saved home page file, since it's more reliable to hae it this way.</param>
        /// <returns></returns>
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

                        group.Boards.Add(new BoardGroup.Board(name, href));
                    }
                    boards.Add(group);
                    if (node != last) group = new BoardGroup();
                }
            }

            return boards;
        }

        public class BoardGroup
        {
            public string? Title;
            public bool IsNSFW;
            public readonly List<Board> Boards = [];

            public record Board(string Title, string URL);
        }
    }
}