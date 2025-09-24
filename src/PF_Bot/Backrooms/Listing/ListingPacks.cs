using System.Text;
using PF_Bot.Core;
using PF_Bot.Core.Text;
using PF_Bot.Routing_New.Routers;

namespace PF_Bot.Backrooms.Listing;

public static class ListingPacks // Did someone said Linkin' Park?
{
    private record FusionListContext(string Title, string Object_Accusative, string CallbackKey, string Marker);

    private static readonly FusionListContext
        PublicPacks  = new("📂 Общие словари" , "словаря", $"{CallbackRouter_Default.Key_Fuse}i",   ""),
        PrivatePacks = new("🔐 Личные словари", "словаря", $"{CallbackRouter_Default.Key_Fuse}!", "! "),
        PublicFiles  = new("📂 Общие файлы" ,   "файла",   $"{CallbackRouter_Default.Key_Fuse}@", "@ "),
        PrivateFiles = new("🔐 Личные файлы",   "файла",   $"{CallbackRouter_Default.Key_Fuse}*", "* ");

    public static void SendPackList(ListPagination pagination, bool fail = false, bool isPrivate = false)
    {
        var fuseList  = isPrivate ? PrivatePacks : PublicPacks;
        var directory = PackManager.GetPacksFolder(pagination.Origin.Chat, isPrivate);
        SendFilesList(fuseList, directory, pagination, fail);
    }

    public static void SendFileList(ListPagination pagination, bool fail = false, bool isPrivate = false)
    {
        var fuseList  = isPrivate ? PrivateFiles : PublicFiles;
        var directory = PackManager.GetFilesFolder(pagination.Origin.Chat, isPrivate);
        SendFilesList(fuseList, directory, pagination, fail);
    }

    private static void SendFilesList
    (
        FusionListContext ctx, FilePath directory, ListPagination pagination, bool fail = false
    )
    {
        var (origin, messageId, page, perPage) = pagination;

        var files = directory.GetFilesInfo();
        var oneshot = files.Length < perPage;

        var lastPage = (int)Math.Ceiling(files.Length / (double)perPage) - 1;
        var sb = new StringBuilder();
        if (fail)
        {
            sb.Append("К сожалению, я не нашёл ").Append(ctx.Object_Accusative).Append(" с таким названием\n\n");
        }

        sb.Append("<b>").Append(ctx.Title).Append(":</b>");
        if (oneshot.Janai()) sb.Append($" 📃{page + 1}/{lastPage + 1}");
        sb.Append("\n\n").AppendJoin('\n', ListFiles(files, ctx.Marker, page, perPage));
        sb.Append("\n\nСловарь <b>этой беседы</b> ");
        var path = PackManager.GetPackPath(origin.Chat);
        if (File.Exists(path))
            sb.Append("весит ").Append(path.FileSizeInBytes.ReadableFileSize());
        else
            sb.Append("пуст");

        if (oneshot.Janai()) sb.Append(USE_ARROWS);

        var buttons = oneshot ? null : Listing.GetPaginationKeyboard(page, perPage, lastPage, ctx.CallbackKey);
        App.Bot.SendOrEditMessage(origin, sb.ToString(), messageId, buttons);
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
}