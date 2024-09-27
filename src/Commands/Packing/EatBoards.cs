using System.Text;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using Witlesss.Backrooms.Types;

#pragma warning disable CS4014

namespace Witlesss.Commands.Packing
{
    //      /boards
    //      /boards info  <┬─ SAME
    //      /board  info  <┘
    //      /board Y-M-D a.N.json
    //      /board [thread/archive/archive link]

    public class EatBoards : Fuse
    {
        private static readonly BoardService _chan = new();
        private static readonly SyncDictionary<long, string> _names = new();

        private static List<BoardService.BoardGroup>? _boards;
        private static List<BoardService.BoardGroup> Boards => _boards ??= _chan.GetBoardList(File_4chanHtmlPage);

        protected override async Task RunAuthorized()
        {
            if (Args is null)
            {
                if (Command!.StartsWith("/boards"))
                    SendBoardList(new ListPagination(Chat, PerPage: 2));
                else
                    Bot.SendMessage(Chat, BOARD_MANUAL);
            }
            else
            {
                if (Args.EndsWith("info"))
                    SendSavedList(new ListPagination(Chat, PerPage: 10));
                else
                    await EatBoard();
            }
        }

        private async Task EatBoard()
        {
            MeasureDick();
            GetWordsPerLineLimit();

            var args = Args.SplitN();
            if (args.Length > 1) await EatJsonFile();
            else                 await EatOnlineData(url: args[0]);
        }

        private async Task EatJsonFile()
        {
            var files = GetFiles(Dir_Board, $"{Args}.json");
            if (files.Length > 0)
            {
                await EatFromJsonFile(files[0]);
                GoodEnding();
            }
            else
                Bot.SendMessage(Chat, FUSE_FAIL_BOARD);
        }

        private async Task EatOnlineData(string url)
        {
            var uri = UrlOrBust(ref url);

            var board = uri.Segments[1].Replace("/", "");

            if (url.EndsWith("/archive"))
            {
                _names[Chat] = $"{board}.zip";

                var threads = _chan.GetArchivedThreads(url);
                var tasks = threads.Select(x => GetDiscussionAsync("https://" + uri.Host + x)).ToList();

                await RespondAndStartEating(tasks);
            }
            else if (url.Contains("/thread/"))
            {
                _names[Chat] = $"{board}.{uri.Segments[3].Replace("/", "")}";

                var replies = _chan.GetThreadDiscussion(url).ToList();

                await EatMany(replies, Context, Size, Limit);
            }
            else // BOARD
            {
                _names[Chat] = board;

                var threads = _chan.GetThreads(url);
                var tasks = threads.Select(x => GetDiscussionAsync(url + x)).ToList();

                await RespondAndStartEating(tasks);
            }
        }

        private Task<List<string>> GetDiscussionAsync(string url)
        {
            // Use .ToList() if u want the Task to start right at this point!
            // Otherwise enumeration will be triggered later (slower).
            return Task.Run(() => _chan.GetThreadDiscussion(url).ToList());
        }

        private async Task RespondAndStartEating(List<Task<List<string>>> tasks)
        {
            var ikuzo = tasks.Count > 60 ? "Начинаю поглощение интернета 😈" : "頂きます！😋🍽";
            var text = string.Format(BOARD_START, tasks.Count, ikuzo);
            if (tasks.Count > 200) text += $"\n\n\n{MAY_TAKE_A_WHILE}";
            var message = Bot.PingChat(Chat, text);
            await Bot.RunOrThrow(EatBoardDiscussion(Context, tasks, Limit), Chat, message);
        }


        private static async Task EatBoardDiscussion(WitlessContext c, List<Task<List<string>>> tasks, int limit)
        {
            await Task.WhenAll(tasks);

            var size = c.Baka.FilePath.FileSizeInBytes();

            var lines = tasks.Select(task => task.Result).SelectMany(s => s).ToList();

            await EatMany(lines, c, size, limit);
        }

        private static async Task EatMany(List<string> lines, WitlessContext c, long size, int limit)
        {
            var (baka, chat, title) = (c.Baka, c.Chat, c.Title);

            var count = baka.Baka.DB.Vocabulary.Count;

            await EatAllLines(lines, baka, limit);
            SaveChanges(baka, title);

            JsonIO.SaveData(lines, GetFileName(chat));

            Bot.SendMessage(chat, FUSION_SUCCESS_REPORT(baka, size, count, title));
        }

        private static string GetFileName(long chat)
        {
            Directory.CreateDirectory(Dir_Board);

            var name = _names[chat];
            _names.Remove(chat);

            var date = IsBoardOrArchive(name)
                ? $"{DateTime.Now:yyyy'-'MM'-'dd' 'HH'.'mm}"
                : $"{DateTime.Now:yyyy'-'MM'-'dd}";
            return Path.Combine(Dir_Board, $"{date} {name}.json");
        }

        private static bool IsBoardOrArchive(string name) => !name.Contains('.') || name.Contains(".zip");

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


        // LISTING

        public new static void HandleCallback(CallbackQuery query, string[] data)
        {
            var pagination = query.GetPagination(data);

            if (data[0] == "b") SendBoardList(pagination);
            else                SendSavedList(pagination);
        }

        private static void SendBoardList(ListPagination pagination)
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

        private static void SendSavedList(ListPagination pagination)
        {
            var (chat, messageId, page, perPage) = pagination;

            var files = GetFilesInfo(Dir_Board).OrderByDescending(x => x.Name).ToArray();

            var single = files.Length <= perPage;

            var lastPage = (int)Math.Ceiling(files.Length / (double)perPage) - 1;
            var sb = new StringBuilder("🍀 <b>Архив досокъ/трѣдовъ:</b> ");
            if (!single) sb.Append("📄[").Append(page + 1).Append('/').Append(lastPage + 1).Append(']');
            sb.Append("\n\n").AppendJoin('\n', JsonList(files, page, perPage));
            if (!single) sb.Append(USE_ARROWS);

            var buttons = single ? null : GetPaginationKeyboard(page, perPage, lastPage, "bi");
            Bot.SendOrEditMessage(chat, sb.ToString(), messageId, buttons);
        }

        private static IEnumerable<string> JsonList(FileInfo[] files, int page = 0, int perPage = 10)
        {
            if (files.Length == 0)
            {
                yield return "*пусто*";
                yield break;
            }

            foreach (var file in files.Skip(page * perPage).Take(perPage))
            {
                var name = file.Name.Replace(".json", "");
                var size = file.Length.ReadableFileSize();
                yield return $"<code>{name}</code> | {size}";

                if (IsBoardOrArchive(name.Split(' ')[^1])) continue;

                yield return $"<blockquote expandable>{GetThreadSubject(file.FullName)}</blockquote>";
            }
        }

        private static string GetThreadSubject(string path)
        {
            var serializer = JsonSerializer;
            using var stream = File.OpenText(path);
            using var reader = new JsonTextReader(stream);

            var text = serializer.Deserialize<List<string>>(reader).First();
            if (text.Contains(':'))
            {
                var s = text.Split(':', 2);
                text = $"<b>{s[0]}</b>:{s[1]}";
            }

            return text;
        }

        private static readonly JsonSerializer JsonSerializer = new()
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
            Converters = { new ThreadSubjectConverter() }
        };

        private class ThreadSubjectConverter : JsonConverter<List<string>>
        {
            public override void WriteJson
                (JsonWriter writer, List<string> value, JsonSerializer serializer)
                => throw new NotImplementedException();

            /// <summary>
            /// Returns only the first string from the list.
            /// </summary>
            public override List<string> ReadJson
                (JsonReader reader, Type type, List<string> list, bool hasValue, JsonSerializer serializer)
            {
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.String) return [(string)reader.Value];
                }

                return [];
            }
        }
    }
}