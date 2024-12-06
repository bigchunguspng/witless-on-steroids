using System.Text;
using Newtonsoft.Json;
using Telegram.Bot.Extensions;
using Telegram.Bot.Types;

#pragma warning disable CS4014

namespace Witlesss.Commands.Packing
{
    //      /boards
    //      /boards info  <┬─ SAME
    //      /board  info  <┘
    //      /board Y-M-D a.N <- [Y-M-D a.N.json]
    //      /board [thread/archive/archive link]

    public class EatBoards : Fuse
    {
        private static readonly BoardService _chan = new();

        private static List<BoardService.BoardGroup>? _boards;
        private static List<BoardService.BoardGroup> Boards => _boards ??= _chan.GetBoardList(File_4chanHtmlPage);

        private string _name = default!;

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

            if      (url.Contains("/thread/")) await EatSingleThread(url, board, uri);
            else if (url.EndsWith("/archive")) await EatArchive     (url, board, uri); 
            else                               await EatWholeBoard  (url, board);
        }

        private async Task EatSingleThread(string url, string board, Uri uri)
        {
            _name = $"{board}.{uri.Segments[3].Replace("/", "")}";

            var replies = _chan.GetThreadDiscussion(url).ToList();

            await EatMany(replies, Size, Limit);
        }

        private async Task EatWholeBoard(string url, string board)
        {
            _name = board;

            var threads = _chan.GetAllActiveThreads(url);
            var tasks = threads.Select(x => _chan.GetThreadDiscussionAsync(url + x));

            await RespondAndStartEating(tasks);
        }

        private async Task EatArchive(string url, string board, Uri uri)
        {
            _name = $"{board}.zip";

            var threads = _chan.GetAllArchivedThreads(url);
            var tasks = threads.Select(x => _chan.GetThreadDiscussionAsync("https://" + uri.Host + x));

            await RespondAndStartEating(tasks);
        }

        private async Task RespondAndStartEating(IEnumerable<Task<List<string>>> tasks)
        {
            var message = Bot.PingChat(Chat, BOARD_START);
            var threads = await Task.WhenAll(tasks);

            Bot.EditMessage(Chat, message, string.Format(BOARD_START_EDIT, threads.Length));

            var size = ChatService.GetPath(Chat).FileSizeInBytes();
            var lines = threads.SelectMany(s => s).ToList();

            await EatMany(lines, size, Limit);
        }

        private async Task EatMany(List<string> lines, long size, int limit)
        {
            var count = Baka.WordCount;

            await EatAllLines(lines, Baka, limit);
            SaveChanges(Baka, Title);

            JsonIO.SaveData(lines, GetFileSavePath());

            Bot.SendMessage(Chat, FUSION_SUCCESS_REPORT(Baka, size, count, Title));
        }

        private string GetFileSavePath()
        {
            Directory.CreateDirectory(Dir_Board);

            var thread = FileNameIsThread(_name);
            var date = thread
                ? $"{DateTime.Now:yyyy'-'MM'-'dd}"
                : $"{DateTime.Now:yyyy'-'MM'-'dd' 'HH'.'mm}";

            return Path.Combine(Dir_Board, $"{date} {_name}.json");
        }

        private static bool FileNameIsThread(string name) => name.Contains('.') && !name.Contains(".zip");

        private Uri UrlOrBust(ref string url)
        {
            try
            {
                if (url.Contains('/') == false) // is a board code e.g. "a" or "g"
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

                if (FileNameIsThread(name.Split(' ')[^1]))
                {
                    yield return $"<blockquote expandable>{GetThreadSubject(file.FullName)}</blockquote>";
                }
            }
        }

        private static readonly Regex _url = new(@"(?:\S+(?::[\/\\])\S+)|(?:<.+\/.*>)", RegexOptions.Compiled);

        private static string GetThreadSubject(string path)
        {
            var serializer = JsonSerializer;
            using var stream = File.OpenText(path);
            using var reader = new JsonTextReader(stream);

            var text = serializer.Deserialize<List<string>>(reader)!.First();
            text = text.Replace("<wbr>", "");
            text = HtmlText.Escape(text);
            text = _url.Replace(text, match => $"<a href=\"{match.Value}\">[deleted]</a>");
            if (text.Contains(": "))
            {
                var s = text.Split(": ", 2);
                text = $"<b>{s[0]}</b>: {s[1]}";
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
                (JsonWriter writer, List<string>? value, JsonSerializer serializer)
                => throw new NotImplementedException();

            /// <summary>
            /// Returns only the first string from the list.
            /// </summary>
            public override List<string> ReadJson
                (JsonReader reader, Type type, List<string>? list, bool hasValue, JsonSerializer serializer)
            {
                while (reader.Read())
                {
                    if (reader.TokenType == JsonToken.String) return [(string)reader.Value!];
                }

                return [];
            }
        }
    }
}