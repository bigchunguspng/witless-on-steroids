using System.Text;
using PF_Bot.Core;
using PF_Bot.Features_Web.Manga;

namespace PF_Bot.Features_Aux.Listing;

public static class ListingManga
{
    private static TCB_Scans_Cache Cache => TCB_Scans_Cache.Instance;

    public static async Task ListMangas
        (ListPagination pagination)
    {
        var (origin, messageId, page, perPage) = pagination;

        var mangas = await Cache.EnsureMangasCached();

        var paginated = mangas.Count > perPage;
        var lastPage = pagination.GetLastPageIndex(mangas.Count);

        var sb = new StringBuilder("🍱 <b>ДОСТУПНЫЕ ТАЙТЛЫ [A-Z]</b>");
        if (paginated) sb.Append($" 📃{page + 1}/{lastPage + 1}");
        sb.Append("\n\n").AppendJoin('\n', GetMangaEntries(mangas, page, perPage));
        if (paginated) sb.Append(USE_ARROWS);

        var buttons = paginated
            ? pagination.GetPaginationKeyboard(lastPage, $"{Registry.CallbackKey_Piece}m")
            : null;
        App.Bot.SendOrEditMessage(origin, sb.ToString(), messageId, buttons);
    }

    public static async Task ListChapters
        (ListPagination pagination, Manga manga)
    {
        var (origin, messageId, page, perPage) = pagination;

        var chapters = await Cache.EnsureChaptersCached(manga);

        var paginated = chapters.Count > perPage;
        var lastPage = pagination.GetLastPageIndex(chapters.Count);

        var sb = new StringBuilder(GetFunnyMangaEmoji(manga.Number));
        sb.Append(" <b>").Append(manga.Title).Append("</b>");
        if (paginated) sb.Append($" 📃{page + 1}/{lastPage + 1}");
        sb.Append("\n\n").AppendJoin('\n', GetChapterEntries(chapters, page, perPage));
        if (paginated) sb.Append(USE_ARROWS);

        var buttons = paginated
            ? pagination.GetPaginationKeyboard(lastPage, $"{Registry.CallbackKey_Piece}c-{manga.Number}")
            : null;
        App.Bot.SendOrEditMessage(origin, sb.ToString(), messageId, buttons);
    }

    private static IEnumerable<string> GetMangaEntries
        (List<Manga> mangas, int page = 0, int perPage = 25)
    {
        if (mangas.Count == 0) return ["*пусто*"];

        return mangas
            .Skip(perPage * page)
            .Take(perPage)
            .Select(manga =>
                $"<blockquote><code>{manga.Code}</code> / <code>{manga.Number}</code>\n"
              + $"<a href='{manga.URL}'>{manga.Title}</a></blockquote>");

    }

    private static IEnumerable<string> GetChapterEntries
        (List<Chapter> chapters, int page, int perPage)
    {
        if (chapters.Count == 0) return ["*пусто*"];

        return chapters
            .Skip(perPage * page)
            .Take(perPage)
            .Select(chapter =>
            {
                var chapter_Title = chapter.ChapterTitle ?? "[...]";
                return $"<code>{chapter.Number}</code> - <a href='{chapter.URL}'>{chapter_Title}</a>";
            });
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