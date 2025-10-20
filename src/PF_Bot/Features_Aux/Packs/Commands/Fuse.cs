using System.Text;
using PF_Bot.Features_Aux.Listing;
using PF_Bot.Features_Aux.Settings.Core;
using PF_Bot.Routing.Callbacks;
using PF_Bot.Routing.Messages.Commands;
using PF_Tools.Copypaster.Helpers;
using Telegram.Bot.Types;

namespace PF_Bot.Features_Aux.Packs.Commands;
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
        var pagination = GetPagination(Content);

        if      (Key == "fi") ListingPacks.SendPackList(pagination);
        else if (Key == "f!") ListingPacks.SendPackList(pagination, isPrivate: true);
        else if (Key == "f@") ListingPacks.SendFileList(pagination);
        else          /* f**/ ListingPacks.SendFileList(pagination, isPrivate: true);
        return Task.CompletedTask;
    }
}

public class Fuse : CommandHandlerAsync_SettingsAsync
{
    protected override async Task RunAuthorized()
    {
        var args = Args.SplitN(2);

        if      (Message.ProvidesFile("text/plain",   out var document)) await ProcessTextAttachment(document);
        if      (Message.ProvidesFile("text/x-ssa",       out document)) await ProcessSubsAttachment(document);
        else if (Message.ProvidesFile("application/json", out document)) await ProcessJsonAttachment(document);
        else if (args.Length == 0)
        {
            SendManual(FUSE_MANUAL);
            Log($"{Title} >> FUSE ?");
        }
        else
        {
            const bool 
                PACK    = true, FILE   = false,
                PRIVATE = true, PUBLIC = false;

            var (pack, isPrivate) = args[0] switch
            {
                "!" => (PACK, PRIVATE),
                "*" => (FILE, PRIVATE),
                "@" => (FILE, PUBLIC),
                _   => (PACK, PUBLIC),
            };

            if (args.Length > 0 && args[^1] == "info")
            {
                var pagination = new ListPagination(Origin);
                if (pack) ListingPacks.SendPackList(pagination, isPrivate);
                else      ListingPacks.SendFileList(pagination, isPrivate);

                var marker = pack && isPrivate.Janai() ? "" : args[0];
                Log($"{Title} >> FUSE INFO {marker}");
            }
            else if (pack) await ProcessFusionRequest(isPrivate, args[^1]);
            else           await ProcessEatingRequest(isPrivate, args.Skip(1));
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
            SetBadStatus();
            Bot.SendSticker(Origin, InputFile.FromFileId(HOLY_MOLY));
        }
        else if (ChatManager.Knowns(chat))
        {
            if (PackManager.BakaIsLoaded(chat, out var otherBaka))
                PackManager.Save(chat, otherBaka);

            await Baka_Fuse_Report(PackManager.GetPackPath(chat));
        }
        else
        {
            SetBadStatus();
            Bot.SendMessage(Origin, FUSE_CHAT_NOT_FOUND);
        }
    }

    private async Task FuseWithPack(bool isPrivate, string arg)
    {
        var files = PackManager.GetPacksFolder(Chat, isPrivate).GetFiles($"{arg}{Ext_Pack}");
        if (files.Length == 0)
        {
            SetBadStatus();
            ListingPacks.SendPackList(new ListPagination(Origin), isPrivate, fail: true);
        }
        else
            await Baka_Fuse_Report(files[0]);
    }

    private async Task ProcessEatingRequest(bool isPrivate, IEnumerable<string> args)
    {
        var name = string.Join(' ', args);

        var files = PackManager.GetFilesFolder(Chat, isPrivate).GetFiles($"{name}.json");
        if (files.Length == 0)
        {
            SetBadStatus();
            ListingPacks.SendFileList(new ListPagination(Origin), isPrivate, fail: true);
        }
        else
            await EatFromJsonFile(files[0]);
    }

    // PROCESS FILE

    private async Task ProcessTextAttachment(Document document)
    {
        var path = await DownloadDocument(document, ".txt", Dir_Temp);
        await EatFromTextFile(path);
    }

    private async Task ProcessSubsAttachment(Document document)
    {
        var path = await DownloadDocument(document, ".ass", Dir_Temp);
        await EatFromSubsFile(path);
    }

    private async Task ProcessJsonAttachment(Document document)
    {
        var dir  = PackManager.GetPrivateFilesFolder(Chat);
        var path = await DownloadDocument(document, ".json", dir);
        try
        {
            await EatFromJsonFile(path);
        }
        catch // wrong format
        {
            File.Delete(path);
            SetBadStatus();
            Bot.SendMessage(Origin, GetJsonFormatExample());
        }
    }

    private async Task<string> DownloadDocument
        (Document document, string extension, FilePath directory)
    {
        var name = document.FileName
                ?? $"fuse-{Desert.GetSand()}{extension}";
        var path = directory
            .EnsureDirectoryExist()
            .Combine(name)
            .MakeUnique();
        await Bot.DownloadFile(document.FileId, path, Origin);
        return path;
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

    private async Task EatFromSubsFile(string path)
    {
        var lines = await File.ReadAllLinesAsync(path);
        var texts = AssParser.ExtractTexts(lines).ToArray();
        await Baka_Eat_Report(texts);
        await SaveJsonCopy(path, texts);
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
        () => Options.MatchNumber().ExtractGroup(0, int.Parse, int.MaxValue);

    private void Log_FUSION
        () => Log($"{Title} >> FUSION DONE", LogLevel.Info, LogColor.Fuchsia);

    private string FUSION_SUCCESS_REPORT
        (FuseReport r) => FUSE_SUCCESS_RESPONSE.Format(Title, r.NewSize, r.DeltaSize, r.DeltaCount);
}