using System.Text;
using PF_Bot.Core;
using PF_Bot.Features_Aux.Packs;

namespace PF_Bot.Features_Aux.Listing;

public static class ListingPacks // Did someone said Linkin' Park?
{
    private record FusionListContext
        (string Title, string Object_Accusative, string CallbackKey, string Marker);

    private static readonly FusionListContext
        PublicPacks  = new("📂 Общие словари" , "словаря", $"{Registry.CallbackKey_Fuse}i",   ""),
        PrivatePacks = new("🔐 Личные словари", "словаря", $"{Registry.CallbackKey_Fuse}!", "! "),
        PublicFiles  = new("📂 Общие файлы" ,   "файла",   $"{Registry.CallbackKey_Fuse}@", "@ "),
        PrivateFiles = new("🔐 Личные файлы",   "файла",   $"{Registry.CallbackKey_Fuse}*", "* ");

    public static void SendPackList
        (ListPagination pagination, bool isPrivate = false, bool fail = false)
    {
        var fuseList  = isPrivate ? PrivatePacks : PublicPacks;
        var directory = PackManager.GetPacksFolder(pagination.Origin.Chat, isPrivate);
        SendFilesList(fuseList, directory, pagination, fail);
    }

    public static void SendFileList
        (ListPagination pagination, bool isPrivate = false, bool fail = false)
    {
        var fuseList  = isPrivate ? PrivateFiles : PublicFiles;
        var directory = PackManager.GetFilesFolder(pagination.Origin.Chat, isPrivate);
        SendFilesList(fuseList, directory, pagination, fail);
    }

    private static void SendFilesList
        (FusionListContext ctx, FilePath directory, ListPagination pagination, bool fail = false)
    {
        var (origin, messageId, page, perPage) = pagination;

        var files = directory.GetFilesInfo();

        var paginated = files.Length > perPage;
        var lastPage = pagination.GetLastPageIndex(files.Length);

        var sb = new StringBuilder();
        if (fail)
            sb
                .Append("К сожалению, я не нашёл ")
                .Append(ctx.Object_Accusative)
                .Append(" с таким названием\n\n");

        sb.Append("<b>").Append(ctx.Title).Append(":</b>");
        if (paginated) sb.Append($" 📃{page + 1}/{lastPage + 1}");
        sb.Append("\n\n").AppendJoin('\n', FormatFiles(files, ctx.Marker, page, perPage));

        sb.Append("\n\nСловарь <b>этой беседы</b> ");
        var path = PackManager.GetPackPath(origin.Chat);
        if (File.Exists(path))
            sb.Append("весит ").Append(path.FileSizeInBytes.ReadableFileSize());
        else
            sb.Append("пуст");

        if (paginated) sb.Append(USE_ARROWS);

        var buttons = paginated
            ? pagination.GetPaginationKeyboard(lastPage, ctx.CallbackKey)
            : null;
        App.Bot.SendOrEditMessage(origin, sb.ToString(), messageId, buttons);
    }

    private static IEnumerable<string> FormatFiles
        (FileInfo[] files, string marker, int page = 0, int perPage = 25)
    {
        if (files.Length == 0) return ["*пусто*"];

        return files
            .Skip(perPage * page)
            .Take(perPage)
            .Select(file =>
            {
                var name = Path.GetFileNameWithoutExtension(file.Name);
                var size = file.Length.ReadableFileSize();
                return $"<code>{marker}{name}</code> | {size}";
            });
    }
}