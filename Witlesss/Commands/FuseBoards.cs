using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;
using Telegram.Bot.Types.ReplyMarkups;

#pragma warning disable CS4014

namespace Witlesss.Commands
{
    public class FuseBoards : WitlessCommand
    {
        private readonly BoardService _chan = new();
        private List<BoardService.BoardGroup> _boards;

        // /board [thread/archive/archive link]
        // /boards  >>  get all boards
        public override void Run()
        {
            if (Bot.ThorRagnarok.ChatIsBanned(Chat)) return;

            if (Text.StartsWith("/boards"))
            {
                SendBoardList(Chat, 0, 2);
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
                Bot.SendMessage(Chat, BOARD_MANUAL);
        }

        public void SendBoardList(long chat, int page, int perPage, int messageId = -1)
        {
            _boards ??= _chan.GetMainMenu("chan.txt");

            var boards = _boards.Skip(page * perPage).Take(perPage);
            var last = (int)Math.Ceiling(_boards.Count / (double)perPage) - 1;
                
            var sb = new StringBuilder("🍀🍀🍀 <b>4CHAN BOARDS</b> 🍀🍀🍀");
            sb.Append(" [PAGE: ").Append(page + 1).Append("/").Append(last + 1).Append("]");
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
            sb.Append("\n\nИспользуйте стрелочки для навигацции ☝️🤓");
            var message = sb.ToString();

            var inactive = InlineKeyboardButton.WithCallbackData("💀", "-");
            var buttons = new List<InlineKeyboardButton>() { inactive, inactive, inactive, inactive };

            if (page > 1       ) buttons[0] = InlineKeyboardButton.WithCallbackData("⏪", CallbackData(0));
            if (page > 0       ) buttons[1] = InlineKeyboardButton.WithCallbackData("⬅️", CallbackData(page - 1));
            if (page < last    ) buttons[2] = InlineKeyboardButton.WithCallbackData("➡️", CallbackData(page + 1));
            if (page < last - 1) buttons[3] = InlineKeyboardButton.WithCallbackData("⏩", CallbackData(last));

            if (messageId < 0)
                Bot.SendMessage(chat, message, new InlineKeyboardMarkup(buttons));
            else
                Bot.EditMessage(chat, messageId, message, new InlineKeyboardMarkup(buttons));

            string CallbackData(int p) => $"b - {p} {perPage}";
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
                Bot.SendMessage(Chat, "Dude, wrong URL 👉😄");
                throw;
            }
        }

        private static async Task EatBoardDiscussion(WitlessCommandParams x, List<Task<List<string>>> tasks)
        {
            await Task.WhenAll(tasks);

            var size = SizeInBytes(x.Baka.Path);

            var lines = tasks.Select(task => task.Result).SelectMany(s => s).ToList();

            EatMany(lines, x.Baka, x.Title);
            SendReport(x.Chat, x.Title, x.Baka, lines.Count, size);
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