using PF_Bot.Core;

namespace PF_Bot.Features_Web.Manga;

public class TCB_Scans_Cache
{
    public static readonly TCB_Scans_Cache Instance = new();

    private readonly TemporaryCache<List<Manga>> MangasCache = new(TimeSpan.FromMinutes(15));
    private readonly Dictionary<string, TemporaryCache<List<Chapter>>> ChaptersCaches = new();

    public async Task<List<Manga>> EnsureMangasCached()
    {
        if (MangasCache.TryGetValue_Failed(out var mangas))
        {
            mangas = await App.TCB.GetTitles();
            MangasCache.Set(mangas);
        }

        return mangas;
    }

    public async Task<List<Chapter>> EnsureChaptersCached(Manga manga)
    {
        var key = manga.Number;
        if (ChaptersCaches.TryGetValue_Failed(key, out var cache))
        {
            cache = new TemporaryCache<List<Chapter>>(TimeSpan.FromMinutes(15));
            ChaptersCaches[key] = cache;
        }

        if (cache.TryGetValue_Failed(out var chapters))
        {
            chapters = await App.TCB.GetChapters(manga.URL);
            chapters.Reverse();
            cache.Set(chapters);
        }

        return chapters;
    }
}