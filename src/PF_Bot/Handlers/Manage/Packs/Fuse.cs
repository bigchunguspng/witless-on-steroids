using System.Text;
using PF_Bot.Backrooms.Helpers;
using PF_Bot.Backrooms.Listing;
using PF_Bot.Core.Chats;
using PF_Bot.Core.Text;
using PF_Bot.Handlers.Manage.Settings;
using PF_Bot.Routing_New.Routers;
using PF_Tools.Copypaster.Helpers;
using Telegram.Bot.Types;

namespace PF_Bot.Handlers.Manage.Packs
{
    //      /fuse
    //      /fuse [JSON / TXT]
    //      /fuse [id   /  name / info]
    //      /fuse [*/!/@] [name / info]

    //      * - history
    //      ! - private packs

    public class Fuse_Callback : CallbackHandler
    {
        protected override Task Run()
        {
            var pagination = Query.GetPagination(Content);

            if      (Key == "fi") ListingPacks.SendPackList(pagination);
            else if (Key == "f!") ListingPacks.SendPackList(pagination, @private: true);
            else if (Key == "f@") ListingPacks.SendFileList(pagination);
            else          /* f**/ ListingPacks.SendFileList(pagination, @private: true);
            return Task.CompletedTask;
        }
    }
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
                if      (_source is FuseSource.PackPublic ) ListingPacks.SendPackList(new ListPagination(Origin));
                else if (_source is FuseSource.PackPrivate) ListingPacks.SendPackList(new ListPagination(Origin), @private: true);
                else if (_source is FuseSource.FilePublic ) ListingPacks.SendFileList(new ListPagination(Origin));
                else if (_source is FuseSource.FilePrivate) ListingPacks.SendFileList(new ListPagination(Origin), @private: true);
            }
            else if (_source is FuseSource.FilePrivate or FuseSource.FilePublic) await ProcessEatingRequest(args);
            else if (_source is FuseSource.PackPrivate or FuseSource.PackPublic) await ProcessFusionRequest(args[^1]);
        }

        
        private static readonly Regex
            _rgx_wordLimit = new(@"^\/\S+?(\d+)", RegexOptions.Compiled);

        protected void GetWordsPerLineLimit()
        {
            Limit = _rgx_wordLimit.ExtractGroup(1, Command!, int.Parse, int.MaxValue);
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
            else if (ChatManager.GetPacksFolder(Chat, @private).GetFiles($"{arg}{Ext_Pack}") is { Length: > 0 } files)
            {
                await FuseWithOtherPack(files[0]);
            }
            else if (argIsChatId) Bot.SendMessage(Origin, FUSE_FAIL_CHAT);
            else ListingPacks.SendPackList(new ListPagination(Origin), fail: true, @private);
        }

        private Task FuseWithOtherPack(FilePath path) => Task.Run(() =>
        {
            Baka.Fuse(GenerationPackIO.Load(path));
            GoodEnding();
        });


        private async Task ProcessEatingRequest(string[] args)
        {
            var @private = _source is FuseSource.FilePrivate;
            var name = string.Join(' ', args.Skip(1));

            var files = ChatManager.GetFilesFolder(Chat, @private).GetFiles($"{name}.json");
            if (files.Length == 0)
            {
                ListingPacks.SendFileList(new ListPagination(Origin), fail: true, @private);
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
            var path = ChatManager.GetPrivateFilesFolder(Chat)
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
            var save = ChatManager.GetPrivateFilesFolder(Chat)
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
}