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
        new PaginatedList<FileInfo>(directory.GetFilesInfo(), ctx.CallbackKey, pagination)
        {
            Header = sb =>
            {
                if (fail)
                    sb.Append($"К сожалению, я не нашёл {ctx.Object_Accusative} с таким названием\n\n");

                sb.Append($"<b>{ctx.Title}:</b>");
            },
            ItemText = (sb, file) =>
            {
                var name = Path.GetFileNameWithoutExtension(file.Name);
                var size = file.Length.ReadableFileSize();
                sb.Append($"<code>{ctx.Marker}{name}</code> | {size}");
            },
            Footer = sb =>
            {
                sb.Append("\n\nСловарь <b>этой беседы</b> ");
                var path = PackManager.GetPackPath(pagination.Origin.Chat);
                if (File.Exists(path))
                    sb.Append("весит ").Append(path.FileSizeInBytes.ReadableFileSize());
                else
                    sb.Append("пуст");
            },
        }.Send();
    }
}