using System.Text;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Witlesss.Commands.Settings;
using Witlesss.Generation.Pack;

namespace Witlesss.Commands.Packing
{
    //      /fuse
    //      /fuse <-[file]
    //      /fuse [id  / name / info]
    //      /fuse [*/!] [name / info]

    //      * - history
    //      ! - private packs

    public class Fuse : AsyncSettingsCommand
    {
        protected long Size;
        private   int Count;
        protected int Limit = int.MaxValue;

        private bool _private, _history;
        private Document? _document;

        protected void MeasureDick() // 😂🤣🤣🤣👌
        {
            Baka.SaveChanges();
            Size = PackPath.FileSizeInBytes();
            Count = Baka.WordCount;
        }

        protected override async Task RunAuthorized()
        {
            MeasureDick();
            GetWordsPerLineLimit();

            var args = Args.SplitN(2);
            _private = args.Length > 1 && args[0] == "!";
            _history = args.Length > 1 && args[0] == "*";

            if      (Message.ProvidesFile("text/plain",       out _document)) await ProcessTextFile();
            else if (Message.ProvidesFile("application/json", out _document)) await ProcessJsonFile();
            else if (args.Length > 0 && args[^1] == "info")
            {
                if      (_history)         SendHistoric(new ListPagination(Chat));
                else if (_private)         SendFuseList(new ListPagination(Chat), @private: true);
                else if (args.Length == 1) SendFuseList(new ListPagination(Chat));
            }
            else if (_history)                     await ProcessEatingRequest(args);
            else if (_private || args.Length == 1) await ProcessFusionRequest(args[^1]);
            else Bot.SendMessage(Chat, FUSE_MANUAL);
        }

        
        private static readonly Regex _wordLimit = new(@"^\/\S+?(\d+)");

        protected void GetWordsPerLineLimit()
        {
            Limit = _wordLimit.ExtractGroup(1, Command!, int.Parse, int.MaxValue);
        }


        #region FUSION

        private const string HOLY_MOLY = "CAACAgIAAxkBAAI062a2Yi7myiBzNob7ftdyivXXEdJjAAKYDAACjdb4SeqLf5UfqK3dNQQ";

        private async Task ProcessFusionRequest(string arg)
        {
            var argIsChatId = long.TryParse(arg, out var chat);
            if (chat == Chat)
            {
                Bot.SendSticker(Chat, new InputOnlineFile(HOLY_MOLY));
                return;
            }

            if (argIsChatId && ChatService.Knowns(chat))
            {
                if (ChatService.BakaIsLoaded(chat, out var baka))
                    baka.SaveChanges();

                await FuseWithOtherPack(ChatService.GetPath(chat));
            }
            else if (GetFiles(GetFuseFolder(Chat, _private), $"{arg}.json") is { Length: > 0 } files)
            {
                await FuseWithOtherPack(files[0]);
            }
            else if (argIsChatId) Bot.SendMessage(Chat, FUSE_FAIL_CHAT);
            else SendFuseList(new ListPagination(Chat), fail: true, @private: _private);
        }

        private Task FuseWithOtherPack(string path) => Task.Run(() =>
        {
            Baka.Fuse(JsonIO.LoadData<GenerationPack>(path));
            GoodEnding();
        });


        private async Task ProcessEatingRequest(string[] args)
        {
            var name = string.Join(' ', args.Skip(1));
            var files = GetFiles(GetHistoryFolder(Chat), $"{name}.json");
            if (files.Length == 0)
            {
                SendHistoric(new ListPagination(Chat), fail: true);
            }
            else
            {
                await EatFromJsonFile(files[0]);
                GoodEnding();
            }
        }

        private async Task ProcessTextFile()
        {
            var path = UniquePath(Dir_History, _document!.FileName ?? "fuse.txt");
            await Bot.DownloadFile(_document.FileId, path, Chat);

            await EatFromTextFile(path);
            GoodEnding();
        }

        private async Task ProcessJsonFile()
        {
            var path = UniquePath(GetHistoryFolder(Chat), _document!.FileName ?? "fuse.json");
            await Bot.DownloadFile(_document.FileId, path, Chat);

            try
            {
                await EatFromJsonFile(path);
                GoodEnding();
            }
            catch // wrong format
            {
                File.Delete(path);
                Bot.SendMessage(Chat, GetJsonFormatExample());
            }
        }

        protected async Task EatFromJsonFile(string path)
        {
            var lines = JsonIO.LoadData<List<string>>(path);
            await EatAllLines(lines);
        }

        private async Task EatFromTextFile(string path)
        {
            var lines = await File.ReadAllLinesAsync(path);
            await EatAllLines(lines);

            SaveJsonCopy(path, lines);
        }

        private void SaveJsonCopy(string path, string[] lines)
        {
            var directory = GetHistoryFolder(Chat);
            Directory.CreateDirectory(directory);

            var name = Path.GetFileNameWithoutExtension(path);
            var save = UniquePath(directory, $"{name}.json");
            JsonIO.SaveData(lines.ToList(), save);
        }

        private                Task      EatAllLines(IEnumerable<string> lines) => EatAllLines(lines, Baka, Limit);
        protected static async Task<int> EatAllLines(IEnumerable<string> lines, CopypasterProxy baka, int limit)
        {
            var linesConsumed = 0;
            await Task.Run(() =>
            {
                foreach (var line in lines.Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    if (line.Count(c => c == ' ' || c == '\n') >= limit) continue;
                    if (baka.Eat(line)) linesConsumed++;
                }
            });
            return linesConsumed;
        }

        protected void GoodEnding()
        {
            SaveChanges(Baka, Title);
            Bot.SendMessage(Chat, FUSION_SUCCESS_REPORT(Baka, Size, Count, Title));
        }

        protected static void SaveChanges(CopypasterProxy baka, string title)
        {
            Log($"{title} >> FUSION DONE", LogLevel.Info, 13);
            baka.Save();
        }

        protected static string FUSION_SUCCESS_REPORT(CopypasterProxy baka, long size, int count, string title)
        {
            var newSize = ChatService.GetPath(baka.Chat).FileSizeInBytes();
            var newCount = baka.WordCount;
            var deltaSize = newSize - size;
            var deltaCount = newCount - count;
            var ns = newSize.ReadableFileSize();
            var ds = deltaSize.ReadableFileSize();
            var dc = BrowseReddit.FormatSubs(deltaCount, "💨");
            return string.Format(FUSE_SUCCESS_RESPONSE, title, ns, ds, dc);
        }

        #endregion


        #region LISTING

        private record FusionListData(string Available, string Object, string Key);

        private static readonly FusionListData ExtraDBp = new("📂 Публичные словари", "словаря", "fi");
        private static readonly FusionListData ExtraDBs = new("🔐 Приватные словари", "словаря", "f!");
        private static readonly FusionListData Historic = new("🔐 Архив файлов",      "файла",   "f*");

        public static void HandleCallback(CallbackQuery query, string[] data)
        {
            var pagination = query.GetPagination(data);

            if (data[0] == "fi") SendFuseList(pagination);
            if (data[0] == "f!") SendFuseList(pagination, @private: true);
            else                 SendHistoric(pagination);
        }

        private static void SendFuseList(ListPagination pagination, bool fail = false, bool @private = false)
        {
            var fuseList  = @private ? ExtraDBs : ExtraDBp;
            var directory = GetFuseFolder(pagination.Chat, @private);
            SendFilesList(fuseList, directory, pagination, fail);
        }

        private static void SendHistoric(ListPagination pagination, bool fail = false)
        {
            var directory = GetHistoryFolder(pagination.Chat);
            SendFilesList(Historic, directory, pagination, fail);
        }

        private static void SendFilesList
        (
            FusionListData data, string directory, ListPagination pagination, bool fail = false
        )
        {
            var (chat, messageId, page, perPage) = pagination;

            var files = GetFilesInfo(directory);
            var oneshot = files.Length < perPage;

            var lastPage = (int)Math.Ceiling(files.Length / (double)perPage) - 1;
            var sb = new StringBuilder();
            if (fail)
            {
                sb.Append("К сожалению, я не нашёл ").Append(data.Object).Append(" с таким названием\n\n");
            }
            sb.Append("<b>").Append(data.Available).Append(":</b>");
            if (!oneshot) sb.Append(" 📄[").Append(page + 1).Append('/').Append(lastPage + 1).Append(']');
            sb.Append("\n\n").AppendJoin('\n', JsonList(files, page, perPage));
            sb.Append("\n\nСловарь <b>этой беседы</b> ");
            var path = ChatService.GetPath(pagination.Chat);
            if (File.Exists(path))
                sb.Append("весит ").Append(path.ReadableFileSize());
            else
                sb.Append("пуст");

            if (!oneshot) sb.Append(USE_ARROWS);

            var buttons = oneshot ? null : GetPaginationKeyboard(page, perPage, lastPage, data.Key);
            Bot.SendOrEditMessage(chat, sb.ToString(), messageId, buttons);
        }

        private static IEnumerable<string> JsonList(FileInfo[] files, int page = 0, int perPage = 25)
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
            }
        }

        #endregion


        private static string GetHistoryFolder(long chat) => Path.Combine(Dir_History, chat.ToString());
        private static string GetPrivateFolder(long chat) => Path.Combine(Dir_Fuse,    chat.ToString());

        private static string GetFuseFolder
            (long chat, bool @private) => @private ? GetPrivateFolder(chat) : Dir_Fuse;

        private string GetJsonFormatExample()
        {
            var sb = new StringBuilder(ONLY_ARRAY_JSON);
            var count = Random.Shared.Next(3, 7);
            sb.Append("\n\n<pre>[");
            for (var i = 0; i < count; i++)
            {
                sb.Append("\n    \"").Append(Baka.Generate()).Append("\"");
                if (i < count - 1) sb.Append(",");
            }
            sb.Append("\n]</pre>");
            return sb.ToString();
        }
    }

    public record ListPagination(long Chat, int MessageId = -1, int Page = 0, int PerPage = 25);
}
