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

    public class Fuse : SettingsCommand
    {
        protected long Size;
        protected int Count;
        protected int Limit = int.MaxValue;

        private bool _private, _history;
        private Document? _document;

        protected void MeasureDick() // 😂🤣🤣🤣👌
        {
            Baka.SaveChanges();
            Size = Baka.FilePath.FileSizeInBytes();
            Count = Baka.Baka.DB.Vocabulary.Count;
        }

        protected override void RunAuthorized()
        {
            MeasureDick();
            GetWordsPerLineLimit();

            var args = Args.SplitN(2);
            _private = args.Length > 1 && args[0] == "!";
            _history = args.Length > 1 && args[0] == "*";

            if      (Message.ProvidesFile("text/plain",       out _document)) ProcessTextFile();
            else if (Message.ProvidesFile("application/json", out _document)) ProcessJsonFile();
            else if (args.Length > 0 && args[^1] == "info")
            {
                if      (_history)         SendHistoric(new ListPagination(Chat));
                else if (_private)         SendFuseList(new ListPagination(Chat), @private: true);
                else if (args.Length == 1) SendFuseList(new ListPagination(Chat));
            }
            else if (_history)                     ProcessEatingRequest(args);
            else if (_private || args.Length == 1) ProcessFusionRequest(args[^1]);
            else Bot.SendMessage(Chat, FUSE_MANUAL);
        }

        
        private static readonly Regex _wordLimit = new(@"^\/\S+?(\d+)");

        protected void GetWordsPerLineLimit()
        {
            Limit = _wordLimit.ExtractGroup(1, Command!, int.Parse, int.MaxValue);
        }


        #region FUSION

        private const string HOLY_MOLY = "CAACAgIAAxkBAAI062a2Yi7myiBzNob7ftdyivXXEdJjAAKYDAACjdb4SeqLf5UfqK3dNQQ";

        private void ProcessFusionRequest(string arg)
        {
            var argIsChatId = long.TryParse(arg, out var chat);
            if (chat == Chat)
            {
                Bot.SendSticker(Chat, new InputOnlineFile(HOLY_MOLY));
                return;
            }

            if (argIsChatId && ChatsDealer.WitlessExist(chat, out var baka))
            {
                if (baka.Loaded) baka.SaveChanges();

                FuseWithOtherPack(JsonIO.LoadData<GenerationPack>(baka.FilePath));
            }
            else if (GetFiles(GetFuseFolder(_private), $"{arg}.json") is { Length: > 0 } files)
            {
                FuseWithOtherPack(JsonIO.LoadData<GenerationPack>(files[0]));
            }
            else if (argIsChatId) Bot.SendMessage(Chat, FUSE_FAIL_CHAT);
            else SendFuseList(new ListPagination(Chat), fail: true, @private: _private);
        }

        private void FuseWithOtherPack(GenerationPack source)
        {
            Baka.Fuse(source);
            GoodEnding();
        }


        private void ProcessEatingRequest(string[] args)
        {
            var name = string.Join(' ', args.Skip(1));
            var files = GetFiles(GetHistoryFolder(), $"{name}.json");
            if (files.Length == 0)
            {
                SendHistoric(new ListPagination(Chat), fail: true);
            }
            else
            {
                EatFromJsonFile(files[0]);
                GoodEnding();
            }
        }

        private void ProcessTextFile()
        {
            var path = UniquePath(Dir_History, _document!.FileName ?? "fuse.txt");
            Bot.DownloadFile(_document.FileId, path, Chat).Wait();

            EatFromTextFile(path);
            GoodEnding();
        }

        private void ProcessJsonFile()
        {
            var path = UniquePath(GetHistoryFolder(), _document!.FileName ?? "fuse.json");
            Bot.DownloadFile(_document.FileId, path, Chat).Wait();

            try
            {
                EatFromJsonFile(path);
                GoodEnding();
            }
            catch // wrong format
            {
                File.Delete(path);
                Bot.SendMessage(Chat, GetJsonFormatExample());
            }
        }

        protected void EatFromJsonFile(string path)
        {
            var lines = JsonIO.LoadData<List<string>>(path);
            EatAllLines(lines);
        }

        private void EatFromTextFile(string path)
        {
            var lines = File.ReadAllLines(path);
            EatAllLines(lines);

            SaveJsonCopy(path, lines);
        }

        private void SaveJsonCopy(string path, string[] lines)
        {
            var directory = GetHistoryFolder();
            Directory.CreateDirectory(directory);

            var name = Path.GetFileNameWithoutExtension(path);
            var save = UniquePath(directory, $"{name}.json");
            JsonIO.SaveData(lines.ToList(), save);
        }

        private          void EatAllLines(IEnumerable<string> lines) => EatAllLines(lines, Baka, Limit, out _);
        protected static void EatAllLines(IEnumerable<string> lines, Witless baka, int limit, out int eaten)
        {
            eaten = 0;
            foreach (var line in lines.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                if (line.Count(c => c == ' ' || c == '\n') >= limit) continue;
                if (baka.Eat(line)) eaten++;
            }
        }

        protected void GoodEnding()
        {
            SaveChanges(Baka, Title);
            Bot.SendMessage(Chat, FUSION_SUCCESS_REPORT(Baka, Size, Count, Title));
        }

        protected static void SaveChanges(Witless baka, string title)
        {
            Log($"{title} >> FUSION DONE", ConsoleColor.Magenta);
            baka.Save();
        }

        protected static string FUSION_SUCCESS_REPORT(Witless baka, long size, int count, string title)
        {
            var newSize = baka.FilePath.FileSizeInBytes();
            var newCount = baka.Baka.DB.Vocabulary.Count;
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

        private readonly FusionListData ExtraDBp = new("📂 Публичные словари", "словаря", "fi");
        private readonly FusionListData ExtraDBs = new("🔐 Приватные словари", "словаря", "f!");
        private readonly FusionListData Historic = new("🔐 Архив файлов",      "файла",   "f*");

        public void HandleCallback(CallbackQuery query, string[] data)
        {
            var pagination = query.GetPagination(data);

            if (data[0] == "fi") SendFuseList(pagination);
            if (data[0] == "f!") SendFuseList(pagination, @private: true);
            else                 SendHistoric(pagination);
        }

        private void SendFuseList(ListPagination pagination, bool fail = false, bool @private = false)
        {
            var fuseList  = @private ? ExtraDBs : ExtraDBp;
            var directory = GetFuseFolder(@private);
            SendFilesList(fuseList, directory, pagination, fail);
        }

        private void SendHistoric(ListPagination pagination, bool fail = false)
        {
            var directory = GetHistoryFolder();
            SendFilesList(Historic, directory, pagination, fail);
        }

        private void SendFilesList
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
            var path = Baka.FilePath;
            if (File.Exists(path))
                sb.Append("весит ").Append(path.ReadableFileSize());
            else
                sb.Append("пуст");

            if (!oneshot) sb.Append(USE_ARROWS);

            var buttons = oneshot ? null : GetPaginationKeyboard(page, perPage, lastPage, data.Key);
            Bot.SendOrEditMessage(chat, sb.ToString(), messageId, buttons);
        }

        protected static IEnumerable<string> JsonList(FileInfo[] files, int page = 0, int perPage = 25)
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


        private string GetHistoryFolder() => Path.Combine(Dir_History, Chat.ToString());
        private string GetPrivateFolder() => Path.Combine(Dir_Fuse,    Chat.ToString());

        private string GetFuseFolder(bool @private) => @private ? GetPrivateFolder() : Dir_Fuse;

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
