using HtmlAgilityPack;

namespace PF_Bot.Features_Web.Manga;

public record Manga
    (string URL, string Title, string Number, string Code);

public record Chapter
    (string URL, string MangaTitle, string? ChapterTitle, string Number);

public class TCB_Scans_Client
{
    private const string
        URL_BASE     = "https://tcbonepiecechapters.com",
        URL_PROJECTS = $"{URL_BASE}/projects";

    private const string
        _xp_ProjectsTitle = "//a[@class='mb-3 text-white text-lg font-bold']",
        _xp_TitleChapter  = "//a[@class='block border border-border bg-card mb-3 p-3 rounded']",
        _xp_ChapterPage   = "//img[@class='fixed-ratio-content']";

    private readonly HtmlWeb _web = new();

    // TITLES

    public async Task<List<Manga>> GetTitles()
    {
        LogDebug("TCB >> TITLES");
        var doc = await _web.LoadFromWebAsync(URL_PROJECTS);
        return doc.DocumentNode.SelectNodes(_xp_ProjectsTitle)
            .Select(x =>
            {
                var url = x.Attributes["href"].Value;
                var bits = url.Split('/', StringSplitOptions.RemoveEmptyEntries);
                var number = bits[1];
                var code   = bits[2];
                return new Manga(URL_BASE + url, x.InnerText, number, code);
            })
            .OrderBy(x => x.Code)
            .ToList();
    }

    // CHAPTER(S)

    public async Task<List<Chapter>> GetChapters(string titleURL)
    {
        LogDebug($"TCB >> CHAPTERS | {TrimURL(titleURL, 8)}");
        var doc = await _web.LoadFromWebAsync(titleURL);
        return doc.DocumentNode.SelectNodes(_xp_TitleChapter)
            .Select(node =>
            {
                var url = node.Attributes["href"].Value;
                var divs = node.ChildNodes.Where(x => x.Name == "div").ToArray();
                var div1 = divs.Length > 0 ? divs[0].InnerText.MakeNull_IfEmpty() : null;
                var div2 = divs.Length > 1 ? divs[1].InnerText.MakeNull_IfEmpty() : null;

                var bits = div1?.Split(" Chapter ") ?? ["Boku no Pico", "1"];
                var   mangaTitle  = bits[0].Trim();
                var chapterNumber = bits[1].Trim();
                var chapterTitle  =   div2?.Trim();
                return new Chapter(URL_BASE + url, mangaTitle, chapterTitle, chapterNumber);
            })
            .ToList();
    }

    // PAGES

    public async Task<List<string>> GetPageURLs(string chapterURL)
    {
        LogDebug($"TCB >> PAGES | {TrimURL(chapterURL, 10)}");
        var doc = await _web.LoadFromWebAsync(chapterURL);
        return doc.DocumentNode.SelectNodes(_xp_ChapterPage)
            .Select(x => x.Attributes["src"].Value)
            .ToList();
    }

    //

    private static ReadOnlySpan<char> TrimURL
        (string text, int offset) => text.AsSpan(URL_BASE.Length + offset);
}