using PF_Bot.Core;
using PF_Bot.Features_Web.Manga;

namespace PF_Bot.Features_Aux.Listing;

public static class ListingManga
{
    private static TCB_Scans_Cache Cache => TCB_Scans_Cache.Instance;

    public static async Task ListMangas
        (ListPagination pagination)
    {
        const string key = $"{Registry.CallbackKey_Piece}m";
        new PaginatedList<Manga>(await Cache.EnsureMangasCached(), key, pagination)
        {
            Header = sb => sb.Append("🍱 <b>ДОСТУПНЫЕ ТАЙТЛЫ [A-Z]</b>"),
            ItemText = (sb, manga) =>
            {
                sb.Append($"<blockquote><code>{manga.Code}</code> / <code>{manga.Number}</code>\n");
                sb.Append($"<a href='{manga.URL}'>{manga.Title}</a></blockquote>");
            },
        }.Send();
    }

    public static async Task ListChapters
        (ListPagination pagination, Manga manga)
    {
        var key = $"{Registry.CallbackKey_Piece}c-{manga.Number}";
        new PaginatedList<Chapter>(await Cache.EnsureChaptersCached(manga), key, pagination)
        {
            Header = sb =>
            {
                sb.Append(GetFunnyMangaEmoji(manga.Number));
                sb.Append(" <b>").Append(manga.Title).Append("</b>");
            },
            ItemText = (sb, chapter) =>
            {
                var chapter_Title = chapter.ChapterTitle ?? "[...]";
                sb.Append($"<code>{chapter.Number}</code> - <a href='{chapter.URL}'>{chapter_Title}</a>");
            },
        }.Send();
    }

    private static readonly string[] _pieces = [ "☠️", "🏴‍☠️", "🌊", "🍖", "🧩" ];

    private static string GetFunnyMangaEmoji(string number) => number switch
    {
        "5" => _pieces.PickAny(),
        "10" => "👊",
        "11" => "🏐",
        "23" => "🕵️‍♂️",
        _ => "🍙",
    };
}