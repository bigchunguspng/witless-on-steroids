using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;

#pragma warning disable CS4014

namespace Witlesss.Commands
{
    public class FuseBoards : WitlessCommand
    {
        private readonly BoardService _chan = new();

        // /board [thread link]
        // /board [archive link]
        // /board [board link]
        // /boards  >>  get all boards
        public override void Run()
        {
            if (Bot.ThorRagnarok.ChatIsBanned(Chat)) return;

            if (Text.StartsWith("/boards"))
            {
                var boards = _chan.GetMainMenu("chan.txt"); // todo .Take(3) to keep formatting
                
                var sb = new StringBuilder("🍀🍀🍀 <b>4CHAN</b> 🍀🍀🍀");
                foreach (var group in boards)
                {
                    sb.Append($"\n\n<b><u>{group.Title}</u></b>");
                    if (group.IsNSFW) sb.Append(" (NSFW🥵)");
                    sb.Append("\n");
                    foreach (var board in group.Boards)
                    {
                        sb.Append($"\n<i>{board.Title}</i> - <code>{board.URL}</code>");
                    }
                }

                sb.Append(string.Format(CheckReddit.SEARCH_FOOTER, Bot.Me.FirstName));
                var result = sb.ToString(); 
                Log(result); // todo del as debug
                Bot.SendMessage(Chat, result);

                return;
            }

            if (Text.Contains(' '))
            {
                var args = Text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                var url = args[1];
                var uri = ValidateUri(url);

                if (url.EndsWith("/archive"))
                {
                    var threads = _chan.GetArchivedThreads(url);
                    var tasks = threads.Select(x => ScrapThreadAsync("https://" + uri.Host + x)).ToList();

                    var message = Bot.PingChat(Chat, string.Format(N_THREADS_FOUND_RESPONSE, tasks.Count));
                    Bot.RunSafelyAsync(EatBoardDiscussion(SnapshotMessageData(), tasks), Chat, message);
                }
                else if (url.Contains("/thread/"))
                {
                    var replies = _chan.GetThreadDisscusion(url).ToList();
                    var size = SizeInBytes(Baka.Path);

                    EatMany(replies, Baka, Title);
                    SendReport(Chat, Title, Baka, replies.Count, size);
                }
                else // BOARD
                {
                    var threads = _chan.GetThreads(url);
                    var tasks = threads.Select(x => ScrapThreadAsync(url + x)).ToList();

                    var message = Bot.PingChat(Chat, string.Format(N_THREADS_FOUND_RESPONSE, tasks.Count));
                    Bot.RunSafelyAsync(EatBoardDiscussion(SnapshotMessageData(), tasks), Chat, message);
                }
            }
            else
                Bot.SendMessage(Chat, "Не можете выбрать? - пропишите <code>/boards</code>\n\nНажали случайно? понимаю 😏");
        }

        private Task<List<string>> ScrapThreadAsync(string url)
        {
            return Task.Run(() => _chan.GetThreadDisscusion(url).ToList()); // ToList() triggers enumeration
        }

        private Uri ValidateUri(string url)
        {
            try
            {
                return new Uri(url);
            }
            catch
            {
                Bot.SendMessage(Chat, "dude, wrong url");
                throw;
            }
        }

        private static async Task EatBoardDiscussion(WitlessCommandParams x, List<Task<List<string>>> tasks)
        {
            Log("wait all start", ConsoleColor.Green);
            await Task.WhenAll(tasks);
            Log("wait all end", ConsoleColor.Red);

            var size = SizeInBytes(x.Baka.Path);

            var lines = tasks.Select(task => task.Result).SelectMany(s => s).ToList();

            EatMany(lines, x.Baka, x.Title);
            SendReport(x.Chat, x.Title, x.Baka, lines.Count, size);
            Log("yumy threads", ConsoleColor.Yellow);
        }

        private static void EatMany(ICollection<string> strings, Witless baka, string title)
        {
            foreach (var text in strings) baka.Eat(text);
            baka.SaveNoMatterWhat();
            Log($"{title} >> {LOG_FUSION_DONE}", ConsoleColor.Magenta);
        }

        private static void SendReport(long chat, string title, Witless baka, int count, long size)
        {
            var newSize = SizeInBytes(baka.Path);
            var difference = FileSize(newSize - size);
            var report = string.Format(FUSE_SUCCESS_RESPONSE, title, FileSize(newSize), difference);
            var detais = $"\n\n Его пополнили {count} строк";
            Bot.SendMessage(chat, report + detais);
        }
    }

    public class BoardService
    {
        private const string BOARD_THREAD = "//a[@class='replylink'][. = 'Reply']";
        private const string ARCHIVE_THREAD = "//a[@class='quotelink']";
        private const string BLOCKQUOTE = "//blockquote";

        private readonly Regex _quote = new(@"<span class=""quote"">([\s\S]+?)<\/span>");
        private readonly HtmlWeb _web = new();


        // https://boards.4channel.org/a/thread/XXX
        public IEnumerable<string> GetThreadDisscusion(string url)
        {
            Log("A --- " + url);
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
            Log("B --- " + url);
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

            var columns = doc.DocumentNode.SelectNodes("//div[@class='column']");
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
            public List<Board> Boards = new();

            public record Board(string Title, string URL);
        }
    }
}