using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
using static System.StringSplitOptions;

#pragma warning disable CS4014

namespace Witlesss.Commands
{
    public class FuseBoards : WitlessCommand
    {
        private readonly BoardService _chan = new();
        private List<BoardService.BoardGroup> _boards;
        private FileInfo[] _files;

        private static readonly SyncronizedDictionary<long, string> _names = new();

        // /boards
        // /boards info
        // /board a.b.c - Y-M-D.json
        // /board [thread/archive/archive link]
        public override void Run()
        {
            if (Bot.ThorRagnarok.ChatIsBanned(Chat)) return;

            if (Text.StartsWith("/boards"))
            {
                if (Text.EndsWith(" info")) SendSavedList(Chat, 0, 25);
                else                        SendBoardList(Chat, 0,  2);

                return;
            }

            if (Text.Contains(' '))
            {
                var args = Text.Split(' ', 2, RemoveEmptyEntries);

                var url = args[1];

                if (url.Contains(' ')) // FUSE WITH JSON FILE
                {
                    var file = $@"{CHAN_FOLDER}\{url}.json";
                    if (GetFiles(CHAN_FOLDER).Contains(file))
                    {
                        var size = SizeInBytes(Baka.Path);
                        
                        var list = new FileIO<List<string>>(file).LoadData();
                        foreach (var line in list) Baka.Eat(line);
                        Baka.SaveNoMatterWhat();
                        Log($"{Title} >> {LOG_FUSION_DONE}", ConsoleColor.Magenta);
                        
                        var newSize = SizeInBytes(Baka.Path);
                        var difference = FileSize(newSize - size);
                        Bot.SendMessage(Chat, string.Format(FUSE_SUCCESS_RESPONSE, Title, FileSize(newSize), difference));
                    }
                    else
                        Bot.SendMessage(Chat, FUSE_FAIL_BOARD);
                    
                    return;
                }

                var uri = UrlOrBust(url);

                var host = uri.Host;
                var name = string.Join('.', url.Split(new[] { host }, None)[1].Split('/', RemoveEmptyEntries).Take(3));
                _names[Chat] = name;

                if (url.EndsWith("/archive"))
                {
                    var threads = _chan.GetArchivedThreads(url);
                    var tasks = threads.Select(x => GetDiscussionAsync("https://" + host + x)).ToList();

                    RespondAndStartEating(tasks);
                }
                else if (url.Contains("/thread/"))
                {
                    var replies = _chan.GetThreadDiscussion(url).ToList();
                    var size = SizeInBytes(Baka.Path);

                    EatMany(replies, Baka, size, Chat, Title);
                }
                else // BOARD
                {
                    var threads = _chan.GetThreads(url);
                    var tasks = threads.Select(x => GetDiscussionAsync(url + x)).ToList();

                    RespondAndStartEating(tasks);
                }
            }
            else
                Bot.SendMessage(Chat, BOARD_MANUAL);
        }

        private Task<List<string>> GetDiscussionAsync(string url)
        {
            // Use IEnumerable<X>.ToList() if u want task to start right at this point!
            // Otherwise enumeration will be triggered later (slower).
            return Task.Run(() => _chan.GetThreadDiscussion(url).ToList());
        }

        private void RespondAndStartEating(List<Task<List<string>>> tasks)
        {
            var less_go = tasks.Count > 60 ? "Начинаю поглощение интернета 😈" : "頂きます！😋🍽";
            var text = string.Format(BOARD_START, tasks.Count, less_go);
            if (tasks.Count > 200) text += $"\n\n\n{MAY_TAKE_A_WHILE}";
            var message = Bot.PingChat(Chat, text);
            Bot.RunSafelyAsync(EatBoardDiscussion(SnapshotMessageData(), tasks), Chat, message);
        }


        private static async Task EatBoardDiscussion(WitlessCommandParams x, List<Task<List<string>>> tasks)
        {
            await Task.WhenAll(tasks);

            var size = SizeInBytes(x.Baka.Path);

            var lines = tasks.Select(task => task.Result).SelectMany(s => s).ToList();

            EatMany(lines, x.Baka, size, x.Chat, x.Title);
        }

        private static void EatMany(List<string> lines, Witless baka, long size, long chat, string title)
        {
            foreach (var text in lines) baka.Eat(text);
            baka.SaveNoMatterWhat();
            Log($"{title} >> {LOG_FUSION_DONE}", ConsoleColor.Magenta);

            Directory.CreateDirectory(CHAN_FOLDER);
            var path = $@"{CHAN_FOLDER}\{_names[chat]} - {DateTime.Now:yyyy'-'MM'-'dd' 'HH'.'mm}.json";
            new FileIO<List<string>>(path).SaveData(lines);
            _names.Remove(chat);

            var newSize = SizeInBytes(baka.Path);
            var difference = FileSize(newSize - size);
            var report = string.Format(FUSE_SUCCESS_RESPONSE, title, FileSize(newSize), difference);
            var detais = $"\n\n<b>Новых строк:</b> {BrowseReddit.FormatSubs(lines.Count, "😏")}";
            Bot.SendMessage(chat, report + detais);
        }


        public void SendBoardList(long chat, int page, int perPage, int messageId = -1)
        {
            _boards ??= _chan.GetBoardList("chan.txt");

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
            sb.Append(string.Format(BrowseReddit.SEARCH_FOOTER, Bot.Me.FirstName));
            sb.Append(USE_ARROWS);

            var text = sb.ToString();
            var buttons = GetPaginationKeyboard(page, perPage, last, "b");

            SendOrEditMessage(chat, text, messageId, buttons);
        }

        public void SendSavedList(long chat, int page, int perPage, int messageId = -1)
        {
            var files = GetFilesInfo(CHAN_FOLDER);
            if (_files is null || _files.Length != files.Length) _files = files;

            var lastPage = (int)Math.Ceiling(_files.Length / (double)perPage) - 1;
            var sb = new StringBuilder("<b>Доступные доскиъ/трѣды:</b> ");
            sb.Append("📄[").Append(page + 1).Append("/").Append(lastPage + 1).Append("]\n");
            sb.Append(Fuse.JsonList(_files, page, perPage));
            sb.Append(USE_ARROWS);

            var text = sb.ToString();
            var buttons = GetPaginationKeyboard(page, perPage, lastPage, "bi");

            SendOrEditMessage(chat, text, messageId, buttons);
        }

        public static void SendOrEditMessage(long chat, string text, int messageId, InlineKeyboardMarkup buttons)
        {
            if (messageId < 0)
                Bot.SendMessage(chat, text, buttons);
            else
                Bot.EditMessage(chat, messageId, text, buttons);
        }

        public static InlineKeyboardMarkup GetPaginationKeyboard(int page, int perPage, int last, string key)
        {
            var inactive = InlineKeyboardButton.WithCallbackData("💀", "-");
            var buttons = new List<InlineKeyboardButton>() { inactive, inactive, inactive, inactive };

            if (page > 1       ) buttons[0] = InlineKeyboardButton.WithCallbackData("⏪", CallbackData(0));
            if (page > 0       ) buttons[1] = InlineKeyboardButton.WithCallbackData("⬅️", CallbackData(page - 1));
            if (page < last    ) buttons[2] = InlineKeyboardButton.WithCallbackData("➡️", CallbackData(page + 1));
            if (page < last - 1) buttons[3] = InlineKeyboardButton.WithCallbackData("⏩", CallbackData(last));

            return new InlineKeyboardMarkup(buttons);
            
            string CallbackData(int p) => $"{key} - {p} {perPage}";
        }


        private Uri UrlOrBust(string url)
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
    }
}