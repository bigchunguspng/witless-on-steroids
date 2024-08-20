using System.Text;
using Telegram.Bot.Types;
using Witlesss.Backrooms.Types;
using static System.StringSplitOptions;

#pragma warning disable CS4014

namespace Witlesss.Commands.Packing
{
    public class EatBoards : Fuse
    {
        private readonly BoardService _chan = new();
        private List<BoardService.BoardGroup>? _boards;
        private FileInfo[]? _files;

        private static readonly SyncronizedDictionary<long, string> _names = new();

        private List<BoardService.BoardGroup> Boards => _boards ??= _chan.GetBoardList("https://www.4chan.org");

        // /boards
        // /boards info
        // /board a.b.c - Y-M-D.json
        // /board [thread/archive/archive link]
        protected override void RunAuthorized()
        {
            if (Command!.StartsWith("/boards"))
            {
                if (Text?.EndsWith(" info") == true) SendSavedList(new ListPagination(Chat));
                else                                 SendBoardList(new ListPagination(Chat, PerPage: 2));

                return;
            }

            if (Args is null)
            {
                Bot.SendMessage(Chat, BOARD_MANUAL);
            }
            else
            {
                MeasureDick();
                GetWordsPerLineLimit();

                var args = Args.SplitN();
                if (args.Length > 1) // FUSE WITH JSON FILE
                {
                    var files = GetFiles(Dir_Board, $"{Args}.json");
                    if (files.Length > 0)
                    {
                        EatFromJsonFile(files[0]);
                        GoodEnding();
                    }
                    else
                        Bot.SendMessage(Chat, FUSE_FAIL_BOARD);

                    return;
                }

                var url = args[0];
                var uri = UrlOrBust(ref url);

                var host = uri.Host;
                var name = string.Join('.', url.Split([host], None)[1].Split('/', RemoveEmptyEntries).Take(3));
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

                    EatMany(replies, Baka, Size, Chat, Title, Limit);
                }
                else // BOARD
                {
                    var threads = _chan.GetThreads(url);
                    var tasks = threads.Select(x => GetDiscussionAsync(url + x)).ToList();

                    RespondAndStartEating(tasks);
                }
            }
        }

        private Task<List<string>> GetDiscussionAsync(string url)
        {
            // Use .ToList() if u want the Task to start right at this point!
            // Otherwise enumeration will be triggered later (slower).
            return Task.Run(() => _chan.GetThreadDiscussion(url).ToList());
        }

        private void RespondAndStartEating(List<Task<List<string>>> tasks)
        {
            var ikuzo = tasks.Count > 60 ? "Начинаю поглощение интернета 😈" : "頂きます！😋🍽";
            var text = string.Format(BOARD_START, tasks.Count, ikuzo);
            if (tasks.Count > 200) text += $"\n\n\n{MAY_TAKE_A_WHILE}";
            var message = Bot.PingChat(Chat, text);
            Bot.RunSafelyAsync(EatBoardDiscussion(Context, tasks, Limit), Chat, message);
        }


        private static async Task EatBoardDiscussion(WitlessContext c, List<Task<List<string>>> tasks, int limit)
        {
            await Task.WhenAll(tasks);

            var size = c.Baka.FilePath.FileSizeInBytes();

            var lines = tasks.Select(task => task.Result).SelectMany(s => s).ToList();

            EatMany(lines, c.Baka, size, c.Chat, c.Title, limit);
        }

        private static void EatMany(List<string> lines, Witless baka, long size, long chat, string title, int limit)
        {
            var count = baka.Baka.DB.Vocabulary.Count;

            EatAllLines(lines, baka, limit, out _);
            SaveChanges(baka, title);

            Directory.CreateDirectory(Dir_Board);
            var path = Path.Combine(Dir_Board, $"{_names[chat]} - {DateTime.Now:yyyy'-'MM'-'dd' 'HH'.'mm}.json");
            JsonIO.SaveData(lines, path);
            _names.Remove(chat);

            Bot.SendMessage(chat, FUSION_SUCCESS_REPORT(baka, size, count, title));
        }


        public new void HandleCallback(CallbackQuery query, string[] data)
        {
            var pagination = query.GetPagination(data);

            if (data[0] == "b") SendBoardList(pagination);
            else                SendSavedList(pagination);
        }

        private void SendBoardList(ListPagination pagination)
        {
            var (chat, messageId, page, perPage) = pagination;

            var boards = Boards.Skip(page * perPage).Take(perPage);
            var last = (int)Math.Ceiling(Boards.Count / (double)perPage) - 1;
                
            var sb = new StringBuilder("🍀🍀🍀 <b>4CHAN BOARDS</b> 🍀🍀🍀");
            sb.Append(" [PAGE: ").Append(page + 1).Append('/').Append(last + 1).Append(']');
            foreach (var group in boards)
            {
                sb.Append($"\n\n<b><u>{group.Title}</u></b>");
                if (group.IsNSFW) sb.Append(" (NSFW🥵)");
                sb.Append('\n');
                foreach (var board in group.Boards)
                {
                    sb.Append($"\n<i>{board.Title}</i> - <code>{board.URL}</code>");
                }
            }
            sb.Append(string.Format(BrowseReddit.SEARCH_FOOTER, Bot.Me.FirstName));
            sb.Append(USE_ARROWS);

            var text = sb.ToString();
            var buttons = GetPaginationKeyboard(page, perPage, last, "b");

            Bot.SendOrEditMessage(chat, text, messageId, buttons);
        }

        private void SendSavedList(ListPagination pagination)
        {
            var (chat, messageId, page, perPage) = pagination;

            var files = GetFilesInfo(Dir_Board);
            if (_files is null || _files.Length != files.Length) _files = files; // todo i think we don't need _files

            var single = _files.Length <= perPage;

            var lastPage = (int)Math.Ceiling(_files.Length / (double)perPage) - 1;
            var sb = new StringBuilder("🍀 <b>Архив досокъ/трѣдовъ:</b> ");
            if (!single) sb.Append("📄[").Append(page + 1).Append('/').Append(lastPage + 1).Append(']');
            sb.Append("\n\n").AppendJoin('\n', JsonList(_files, page, perPage));
            if (!single) sb.Append(USE_ARROWS);

            var buttons = single ? null : GetPaginationKeyboard(page, perPage, lastPage, "bi");
            Bot.SendOrEditMessage(chat, sb.ToString(), messageId, buttons);
        }


        private Uri UrlOrBust(ref string url)
        {
            try
            {
                if (!url.Contains('/'))
                {
                    var ending = $"/{url}/";
                    var urls = Boards.SelectMany(x => x.Boards.Select(b => b.URL)).ToList();
                    var match = urls.FirstOrDefault(x => x.EndsWith(ending));
                    if (match != null)
                    {
                        url = match;
                    }
                }

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