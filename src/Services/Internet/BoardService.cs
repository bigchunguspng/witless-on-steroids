﻿using System.Web;
using HtmlAgilityPack;

namespace Witlesss.Services.Internet
{
    public class BoardService
    {
        private const string BOARD_THREAD = "//a[@class='replylink'][. = 'Reply']";
        private const string ARCHIVE_THREAD = "//a[@class='quotelink']";
        private const string BLOCKQUOTE = "//blockquote";
        private const string SUBJECT = "//span[@class='subject']";
        private const string COLUMN = "//div[@class='column']";

        private readonly Regex _quote = new(@"<span class=""quote"">([\s\S]+?)<\/span>");
        private readonly HtmlWeb _web = new();


        /// <summary> Returns every single line of anons discussion. </summary>
        /// <param name="url">URL, like https://boards.4channel.org/a/thread/XXX</param>
        public IEnumerable<string> GetThreadDiscussion(string url)
        {
            var document = GetDocument(url);
            var subject = document.SelectSingleNode(SUBJECT).InnerHtml;
            var subjectPending = !string.IsNullOrWhiteSpace(subject);
            foreach (var block in document.SelectNodes(BLOCKQUOTE))
            {
                var lines = block.InnerHtml.Split("<br>", StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (line.StartsWith("<a")) continue;

                    var text = _quote.ExtractGroup(1, line, s => s, line)!;

                    text = text.Replace("<s>", "").Replace("</s>", "");
                    if (subjectPending)
                    {
                        text = $"{subject}: {text}";
                        subjectPending = false;
                    }

                    yield return HttpUtility.HtmlDecode(text);
                }
            }
        }

        /// <summary> Returns thread URLs from a board page. </summary>
        /// <param name="url">URL, like https://boards.4channel.org/a/</param>
        public IEnumerable<string> GetThreads(string url)
        {
            return GetHrefs(url, BOARD_THREAD);
        }

        /// <summary> Returns ARCHIVED thread URLs from a board ARCHIVE page. </summary>
        /// <param name="url">URL, like https://boards.4channel.org/a/archive</param>
        public IEnumerable<string> GetArchivedThreads(string url)
        {
            return GetHrefs(url, ARCHIVE_THREAD);
        }

        private IEnumerable<string> GetHrefs(string url, string xpath)
        {
            return GetDocument(url).SelectNodes(xpath).Select(x => x.Attributes["href"].Value);
        }
        private HtmlNode GetDocument(string url) => _web.Load(url).DocumentNode;


        public List<BoardGroup> GetBoardList(string path)
        {
            var boards = new List<BoardGroup>();
            var group = new BoardGroup();

            var document = new HtmlDocument();
            document.Load(path);

            var columns = document.DocumentNode.SelectNodes(COLUMN);
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
            public string Title;
            public bool IsNSFW;
            public readonly List<Board> Boards = new();

            public record Board(string Title, string URL);
        }
    }
}