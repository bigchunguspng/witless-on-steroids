using System.Text;
using PF_Bot.Backrooms.Helpers;
using PF_Bot.Commands;
using PF_Bot.Features_Aux.Listing;
using PF_Bot.Features_Aux.Packs.Core;
using PF_Bot.Features_Aux.Settings.Core;
using PF_Bot.Routing_New.Routers;
using PF_Tools.Copypaster.Helpers;
using Telegram.Bot.Types;

namespace PF_Bot.Features_Aux.Packs.Commands
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
            else if (Key == "f!") ListingPacks.SendPackList(pagination, isPrivate: true);
            else if (Key == "f@") ListingPacks.SendFileList(pagination);
            else          /* f**/ ListingPacks.SendFileList(pagination, isPrivate: true);
            return Task.CompletedTask;
        }
    }

    public class Fuse : AsyncSettingsCommand
    {
        [Flags]
        private enum FuseSource
        {
            Pack    = 1,
            File    = 2,
            Public  = 4,
            Private = 8,
            PackPublic  = Pack | Public,
            PackPrivate = Pack | Private,
            FilePublic  = File | Public,
            FilePrivate = File | Private,
        }

        protected override async Task RunAuthorized()
        {
            var args = Args.SplitN(2);

            if      (Message.ProvidesFile("text/plain",   out var document)) await ProcessTextAttachment(document);
            else if (Message.ProvidesFile("application/json", out document)) await ProcessJsonAttachment(document);
            else if (args.Length == 0)
            {
                Bot.SendMessage(Origin, FUSE_MANUAL);
                Log($"{Title} >> FUSE ?");
            }
            else
            {
                var _source = args[0] switch
                {
                    "!" => FuseSource.PackPrivate,
                    "*" => FuseSource.FilePrivate,
                    "@" => FuseSource.FilePublic,
                    _   => FuseSource.PackPublic,
                };

                var pack      = (_source & FuseSource.Pack)    == FuseSource.Pack;
                var isPrivate = (_source & FuseSource.Private) == FuseSource.Private;

                if (args.Length > 0 && args[^1] == "info")
                {
                    var pagination = new ListPagination(Origin);
                    if (pack) ListingPacks.SendPackList(pagination, isPrivate);
                    else      ListingPacks.SendFileList(pagination, isPrivate);

                    Log($"{Title} >> FUSE INFO {(_source == FuseSource.PackPublic ? "" : args[0])}");
                }
                else if (pack) await ProcessFusionRequest(isPrivate, args[^1]);
                else           await ProcessEatingRequest(isPrivate, args);
            }
        }


        private const string
            HOLY_MOLY = "CAACAgIAAxkBAAI062a2Yi7myiBzNob7ftdyivXXEdJjAAKYDAACjdb4SeqLf5UfqK3dNQQ";

        private async Task ProcessFusionRequest(bool isPrivate, string arg)
        {
            var c = long.TryParse(arg, out var chat);
            if (c) await FuseWithChat(chat);
            else   await FuseWithPack(isPrivate, arg);
        }

        private async Task FuseWithChat(long chat)
        {
            if (chat == Chat)
            {
                Bot.SendSticker(Origin, InputFile.FromFileId(HOLY_MOLY));
            }
            else if (ChatManager.Knowns(chat))
            {
                if (PackManager.BakaIsLoaded(chat, out var otherBaka))
                    PackManager.Save(chat, otherBaka);

                await Baka_Fuse_Report(PackManager.GetPackPath(chat));
            }
            else
                Bot.SendMessage(Origin, FUSE_CHAT_NOT_FOUND);
        }

        private async Task FuseWithPack(bool isPrivate, string arg)
        {
            var files = PackManager.GetPacksFolder(Chat, isPrivate).GetFiles($"{arg}{Ext_Pack}");
            if (files.Length > 0)
                await Baka_Fuse_Report(files[0]);
            else
                ListingPacks.SendPackList(new ListPagination(Origin), isPrivate, fail: true);
        }

        private async Task ProcessEatingRequest(bool isPrivate, string[] args)
        {
            var name = string.Join(' ', args.Skip(1));

            var files = PackManager.GetFilesFolder(Chat, isPrivate).GetFiles($"{name}.json");
            if (files.Length == 0)
            {
                ListingPacks.SendFileList(new ListPagination(Origin), isPrivate, fail: true);
            }
            else
            {
                await EatFromJsonFile(files[0]);
            }
        }

        // PROCESS FILE

        private async Task ProcessTextAttachment(Document document)
        {
            var path = Dir_Temp
                .EnsureDirectoryExist()
                .Combine(document.FileName ?? "fuse.txt")
                .MakeUnique();
            await Bot.DownloadFile(document.FileId, path, Origin);
            await EatFromTextFile(path);
        }

        private async Task ProcessJsonAttachment(Document document)
        {
            var path = PackManager.GetPrivateFilesFolder(Chat)
                .EnsureDirectoryExist()
                .Combine(document.FileName ?? "fuse.json")
                .MakeUnique();
            await Bot.DownloadFile(document.FileId, path, Origin);
            try
            {
                await EatFromJsonFile(path);
            }
            catch // wrong format
            {
                File.Delete(path);
                Bot.SendMessage(Origin, GetJsonFormatExample());
            }
        }

        private string GetJsonFormatExample()
        {
            var count = Random.Shared.Next(3, 7);
            var sb = new StringBuilder(ONLY_ARRAY_JSON).Append("\n\n<pre>[");
            for (var i = 0; i < count; i++)
                sb.Append("\n    ").AppendInQuotes(Baka.Generate()).Append(",");
            return sb.Remove(sb.Length - 1, 1).Append("\n]</pre>").ToString();
        }

        // EAT FROM FILE

        protected async Task EatFromJsonFile(string path)
        {
            var lines = await JsonIO.LoadDataAsync<List<string>>(path);
            await Baka_Eat_Report(lines);
        }

        private async Task EatFromTextFile(string path)
        {
            var lines = await File.ReadAllLinesAsync(path);
            await Baka_Eat_Report(lines);
            await SaveJsonCopy(path, lines);
        }

        private async Task SaveJsonCopy(FilePath path, string[] lines)
        {
            var save = PackManager.GetPrivateFilesFolder(Chat)
                .EnsureDirectoryExist()
                .Combine($"{path.FileNameWithoutExtension}.json")
                .MakeUnique();
            await JsonIO.SaveDataAsync(lines, save);
        }

        // CORE + LOGS

        private async Task Baka_Fuse_Report(FilePath path)
        {
            var report = await Task.Run(() => PackManager.Fuse(Chat, Baka, GenerationPackIO.Load(path)));
            Log_FUSION();
            Bot.SendMessage(Origin, FUSION_SUCCESS_REPORT(report));
        }

        private async Task Baka_Eat_Report(IEnumerable<string> lines)
        {
            var feed = await Task.Run(() => PackManager.Feed(Chat, Baka, lines, GetWordsPerLineLimit()));
            Log_FUSION();
            Bot.SendMessage(Origin, FUSION_SUCCESS_REPORT(feed.Report));
        }

        protected async Task Baka_Eat_Report(List<string> lines, string path, Func<FeedReport, string?> getDetails)
        {
            var feed = await Task.Run(() => PackManager.Feed(Chat, Baka, lines, GetWordsPerLineLimit()));
            Log_FUSION();

            await JsonIO.SaveDataAsync(lines, path);

            var report = FUSION_SUCCESS_REPORT(feed.Report);
            Bot.SendMessage(Origin, $"{report}{getDetails(feed)}");
        }

        private int GetWordsPerLineLimit
            () => Command!.MatchNumber().ExtractGroup(0, int.Parse, int.MaxValue);

        private void Log_FUSION
            () => Log($"{Title} >> FUSION DONE", LogLevel.Info, LogColor.Fuchsia);

        private string FUSION_SUCCESS_REPORT
            (FuseReport r) => string.Format(FUSE_SUCCESS_RESPONSE, Title, r.NewSize, r.DeltaSize, r.DeltaCount);
    }
}