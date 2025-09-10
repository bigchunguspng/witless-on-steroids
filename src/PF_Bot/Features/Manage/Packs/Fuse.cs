using System.Text;
using PF_Bot.Backrooms.Helpers;
using PF_Bot.Core.Chats;
using PF_Bot.Core.Text;
using PF_Bot.Features.Manage.Settings;
using Telegram.Bot.Types;

namespace PF_Bot.Features.Manage.Packs
{
    //      /fuse
    //      /fuse [JSON / TXT]
    //      /fuse [id   /  name / info]
    //      /fuse [*/!/@] [name / info]

    //      * - history
    //      ! - private packs

    public class Fuse : AsyncSettingsCommand
    {
        protected long Size;
        private   int Count;
        protected int Limit = int.MaxValue;

        private Document? _document;

        private      FuseSource _source;
        private enum FuseSource { PackPublic, PackPrivate, FilePublic, FilePrivate }

        protected void MeasureDick() // 😂🤣🤣🤣👌
        {
            ChatManager.SaveBaka(Chat, Baka);
            Size = PackPath.FileSizeInBytes;
            Count = Baka.VocabularyCount;
        }

        protected override async Task RunAuthorized()
        {
            MeasureDick();
            GetWordsPerLineLimit();

            var args = Args.SplitN(2);
            _source =
                args.Length > 1 && args[0] == "!" ? FuseSource.PackPrivate :
                args.Length > 1 && args[0] == "*" ? FuseSource.FilePrivate :
                args.Length > 1 && args[0] == "@" ? FuseSource.FilePublic : FuseSource.PackPublic;

            if      (Message.ProvidesFile("text/plain",       out _document)) await ProcessTextFile();
            else if (Message.ProvidesFile("application/json", out _document)) await ProcessJsonFile();
            else if (args.Length == 0) Bot.SendMessage(Origin, FUSE_MANUAL);
            else if (args.Length  > 0 && args[^1] == "info")
            {
                if      (_source is FuseSource.PackPublic ) SendPackList(new ListPagination(Origin));
                else if (_source is FuseSource.PackPrivate) SendPackList(new ListPagination(Origin), @private: true);
                else if (_source is FuseSource.FilePublic ) SendFileList(new ListPagination(Origin));
                else if (_source is FuseSource.FilePrivate) SendFileList(new ListPagination(Origin), @private: true);
            }
            else if (_source is FuseSource.FilePrivate or FuseSource.FilePublic) await ProcessEatingRequest(args);
            else if (_source is FuseSource.PackPrivate or FuseSource.PackPublic) await ProcessFusionRequest(args[^1]);
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
            var @private = _source is FuseSource.PackPrivate;

            var argIsChatId = long.TryParse(arg, out var chat);
            if (chat == Chat)
            {
                Bot.SendSticker(Origin, InputFile.FromFileId(HOLY_MOLY));
                return;
            }

            if (argIsChatId && ChatManager.KnownsChat(chat))
            {
                if (ChatManager.BakaIsLoaded(chat, out var baka))
                    ChatManager.SaveBaka(chat, baka);

                await FuseWithOtherPack(ChatManager.GetPackPath(chat));
            }
            else if (GetPacksFolder(Chat, @private).GetFiles($"{arg}{Ext_Pack}") is { Length: > 0 } files)
            {
                await FuseWithOtherPack(files[0]);
            }
            else if (argIsChatId) Bot.SendMessage(Origin, FUSE_FAIL_CHAT);
            else SendPackList(new ListPagination(Origin), fail: true, @private);
        }

        private Task FuseWithOtherPack(string path) => Task.Run(() =>
        {
            Baka.Fuse(GenerationPackIO.Load(path));
            GoodEnding();
        });


        private async Task ProcessEatingRequest(string[] args)
        {
            var @private = _source is FuseSource.FilePrivate;
            var name = string.Join(' ', args.Skip(1));

            var files = GetFilesFolder(Chat, @private).GetFiles($"{name}.json");
            if (files.Length == 0)
            {
                SendFileList(new ListPagination(Origin), fail: true, @private);
            }
            else
            {
                await EatFromJsonFile(files[0]);
                GoodEnding();
            }
        }

        private async Task ProcessTextFile()
        {
            var path = Dir_Temp
                .EnsureDirectoryExist()
                .Combine(_document!.FileName ?? "fuse.txt")
                .MakeUnique();
            await Bot.DownloadFile(_document.FileId, path, Origin);

            await EatFromTextFile(path);
            GoodEnding();
        }

        private async Task ProcessJsonFile()
        {
            var path = GetPrivateFilesFolder(Chat)
                .EnsureDirectoryExist()
                .Combine(_document!.FileName ?? "fuse.json")
                .MakeUnique();
            await Bot.DownloadFile(_document.FileId, path, Origin);

            try
            {
                await EatFromJsonFile(path);
                GoodEnding();
            }
            catch // wrong format
            {
                File.Delete(path);
                Bot.SendMessage(Origin, GetJsonFormatExample());
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

        private void SaveJsonCopy(FilePath path, string[] lines)
        {
            var save = GetPrivateFilesFolder(Chat)
                .EnsureDirectoryExist()
                .Combine(path.ChangeExtension(".json"))
                .MakeUnique();
            JsonIO.SaveData(lines.ToList(), save);
        }

        private                Task      EatAllLines(IEnumerable<string> lines) => EatAllLines(lines, Baka, Limit);
        protected static async Task<int> EatAllLines(IEnumerable<string> lines, Copypaster baka, int limit)
        {
            var linesConsumed = 0;
            await Task.Run(() =>
            {
                foreach (var line in lines.Where(x => x.IsNotNull_NorWhiteSpace()))
                {
                    if (line.Count(c => c == ' ' || c == '\n') >= limit) continue;
                    if (baka.Eat(line)) linesConsumed++;
                }
            });
            return linesConsumed;
        }

        protected void GoodEnding()
        {
            SaveChanges(Baka, Chat, Title);
            Bot.SendMessage(Origin, FUSION_SUCCESS_REPORT(Baka, Chat, Size, Count, Title));
        }

        protected static void SaveChanges(Copypaster baka, long chat, string title)
        {
            Log($"{title} >> FUSION DONE", LogLevel.Info, LogColor.Fuchsia);
            ChatManager.SaveBaka(chat, baka);
        }

        protected static string FUSION_SUCCESS_REPORT(Copypaster baka, long chat, long size, int count, string title)
        {
            var newSize = ChatManager.GetPackPath(chat).FileSizeInBytes;
            var newCount = baka.VocabularyCount;
            var deltaSize = newSize - size;
            var deltaCount = newCount - count;
            var ns = newSize.ReadableFileSize();
            var ds = deltaSize.ReadableFileSize();
            var dc = deltaCount.Format_bruh_1k_100k_1M("💨");
            return string.Format(FUSE_SUCCESS_RESPONSE, title, ns, ds, dc);
        }

        #endregion


        #region LISTING

        private record FusionListData(string Title, string Object, string Key, string Marker);

        private static readonly FusionListData PublicPacks  = new("📂 Общие словари" , "словаря", "fi",   "");
        private static readonly FusionListData PrivatePacks = new("🔐 Личные словари", "словаря", "f!", "! ");
        private static readonly FusionListData PublicFiles  = new("📂 Общие файлы" ,   "файла",   "f@", "@ ");
        private static readonly FusionListData PrivateFiles = new("🔐 Личные файлы",   "файла",   "f*", "* ");

        public static void HandleCallback(CallbackQuery query, string[] data)
        {
            var pagination = query.GetPagination(data);

            if      (data[0] == "fi") SendPackList(pagination);
            else if (data[0] == "f!") SendPackList(pagination, @private: true);
            else if (data[0] == "f@") SendFileList(pagination);
            else              /* f* */SendFileList(pagination, @private: true);
        }

        private static void SendPackList(ListPagination pagination, bool fail = false, bool @private = false)
        {
            var fuseList  = @private ? PrivatePacks : PublicPacks;
            var directory = GetPacksFolder(pagination.Origin.Chat, @private);
            SendFilesList(fuseList, directory, pagination, fail);
        }

        private static void SendFileList(ListPagination pagination, bool fail = false, bool @private = false)
        {
            var fuseList  = @private ? PrivateFiles : PublicFiles;
            var directory = GetFilesFolder(pagination.Origin.Chat, @private);
            SendFilesList(fuseList, directory, pagination, fail);
        }

        private static void SendFilesList
        (
            FusionListData data, FilePath directory, ListPagination pagination, bool fail = false
        )
        {
            var (origin, messageId, page, perPage) = pagination;

            var files = directory.GetFilesInfo();
            var oneshot = files.Length < perPage;

            var lastPage = (int)Math.Ceiling(files.Length / (double)perPage) - 1;
            var sb = new StringBuilder();
            if (fail)
            {
                sb.Append("К сожалению, я не нашёл ").Append(data.Object).Append(" с таким названием\n\n");
            }
            sb.Append("<b>").Append(data.Title).Append(":</b>");
            if (!oneshot) sb.Append($" 📃{page + 1}/{lastPage + 1}");
            sb.Append("\n\n").AppendJoin('\n', ListFiles(files, data.Marker, page, perPage));
            sb.Append("\n\nСловарь <b>этой беседы</b> ");
            var path = ChatManager.GetPackPath(origin.Chat);
            if (File.Exists(path))
                sb.Append("весит ").Append(path.FileSizeInBytes.ReadableFileSize());
            else
                sb.Append("пуст");

            if (!oneshot) sb.Append(USE_ARROWS);

            var buttons = oneshot ? null : GetPaginationKeyboard(page, perPage, lastPage, data.Key);
            Bot.SendOrEditMessage(origin, sb.ToString(), messageId, buttons);
        }

        private static IEnumerable<string> ListFiles(FileInfo[] files, string marker, int page = 0, int perPage = 25)
        {
            if (files.Length == 0)
            {
                yield return "*пусто*";
                yield break;
            }

            foreach (var file in files.Skip(page * perPage).Take(perPage))
            {
                var name = Path.GetFileNameWithoutExtension(file.Name);
                var size = file.Length.ReadableFileSize();
                yield return $"<code>{marker}{name}</code> | {size}";
            }
        }

        #endregion


        private static FilePath GetPrivatePacksFolder(long chat) => Dir_Fuse   .Combine(chat.ToString());
        private static FilePath GetPrivateFilesFolder(long chat) => Dir_History.Combine(chat.ToString());

        private static FilePath GetPacksFolder
            (long chat, bool @private) => @private ? GetPrivatePacksFolder(chat) : Dir_Fuse;

        private static FilePath GetFilesFolder
            (long chat, bool @private) => @private ? GetPrivateFilesFolder(chat) : Dir_History;

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

    public record ListPagination(MessageOrigin Origin, int MessageId = -1, int Page = 0, int PerPage = 25);
}
