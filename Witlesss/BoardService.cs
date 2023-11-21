using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;

namespace Witlesss
{
    public class BoardService
    {
        private const string BOARD_THREAD = "//a[@class='replylink'][. = 'Reply']";
        private const string ARCHIVE_THREAD = "//a[@class='quotelink']";
        private const string BLOCKQUOTE = "//blockquote";
        private const string COLUMN = "//div[@class='column']";

        private readonly Regex _quote = new(@"<span class=""quote"">([\s\S]+?)<\/span>");
        private readonly HtmlWeb _web = new();


        // https://boards.4channel.org/a/thread/XXX
        public IEnumerable<string> GetThreadDisscusion(string url)
        {
            var doc = _web.Load(url);

            var blockquotes = doc.DocumentNode.SelectNodes(BLOCKQUOTE);

            foreach (var block in blockquotes)
            {
                var lines = block.InnerHtml.Split("<br>", StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (line.StartsWith("<a")) continue;

                    var match = _quote.Match(line);
                    var text = match.Success ? match.Groups[1].Value : line;

                    yield return HttpUtility.HtmlDecode(text.Replace("<s>", "").Replace("</s>", ""));
                }
            }
        }

        // https://boards.4channel.org/a/ >> thread/259670255/onepiece
        public IEnumerable<string> GetThreads(string url)
        {
            var doc = _web.Load(url);

            return doc.DocumentNode.SelectNodes(BOARD_THREAD).Select(x => x.Attributes["href"].Value);
        }

        // https://boards.4channel.org/a/archive >> /a/thread/XXX/le-text
        public IEnumerable<string> GetArchivedThreads(string url)
        {
            var doc = _web.Load(url);

            return doc.DocumentNode.SelectNodes(ARCHIVE_THREAD).Select(x => x.Attributes["href"].Value);
        }

        public List<BoardGroup> GetMainMenu(string path)
        {
            var doc = new HtmlDocument();
            doc.Load(path);

            var boards = new List<BoardGroup>();
            var group = new BoardGroup();

            var columns = doc.DocumentNode.SelectNodes(COLUMN);
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
                        var href = "https:" + item.ChildNodes[0].Attributes["href"].Value;

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
            public string Title;
            public bool IsNSFW;
            public readonly List<Board> Boards = new();

            public record Board(string Title, string URL);
        }
    }
}