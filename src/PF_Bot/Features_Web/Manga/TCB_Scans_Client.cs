using HtmlAgilityPack;

namespace PF_Bot.Features_Web.Manga;

public class TCB_Scans_Client
{
    private const string
        URL_BASE     = "https://tcbonepiecechapters.com",
        URL_PROJECTS = $"{URL_BASE}/projects";

    private const string
        _xp_ProjectsTitle = "//a[@class='mb-3 text-white text-lg font-bold']",
        _xp_TitleChapter  = "//a[@class='block border border-border bg-card mb-3 p-3 rounded']",
        _xp_TitleName     = "//h1[@class='my-3 font-bold text-3xl']",
        _xp_ChapterPage   = "//img[@class='fixed-ratio-content']";

    private readonly HtmlWeb _web = new();

    /// Return format: /mangas/5/one-piece
    public async Task<string?> GetTitleURL(string title)
    {
        LogDebug($"TCB >> TITLE | {title}");
        var doc = await _web.LoadFromWebAsync(URL_PROJECTS);
        return doc.DocumentNode.SelectNodes(_xp_ProjectsTitle)
            .OrderBy(x => x.InnerText)
            .Select(x => x.Attributes["href"].Value)
            .FirstOrDefault(x => x.AsSpan(x.LastIndexOf('/') + 1).StartsWith(title, StringComparison.OrdinalIgnoreCase));
    }

    public record Chapter(string URL, string MangaTitle, string? ChapterTitle, string Number);

    /// Return format (Chapter.URL): /chapters/7899/one-piece-chapter-1162
    public async Task<Chapter?> GetChapterInfo(string titleURL, string number)
    {
        LogDebug($"TCB >> CHAPTER | {titleURL.AsSpan(8)}, {number}");
        var doc = await _web.LoadFromWebAsync(URL_BASE + titleURL);
        var node = doc.DocumentNode.SelectNodes(_xp_TitleChapter)
            .FirstOrDefault(x => x.ChildNodes.First(x1 => x1.Name == "div").InnerText.EndsWith(number));

        if (node is null) return null;

        var url = node.Attributes["href"].Value;
        var divs = node.ChildNodes.Where(x => x.Name == "div").ToArray();
        var div1 = divs.Length > 0 ? divs[0].InnerText.MakeNull_IfEmpty() : null;
        var div2 = divs.Length > 1 ? divs[1].InnerText.MakeNull_IfEmpty() : null;

        var bits = div1?.Split(" Chapter ") ?? ["Boku no Pico", "1"];
        var   mangaTitle  = bits[0].Trim();
        var chapterNumber = bits[1].Trim();
        var chapterTitle  =   div2?.Trim();
        return new Chapter(url, mangaTitle, chapterTitle, chapterNumber);
    }

    /// Return format: Full URLs.
    public async Task<List<string>> GetPageURLs(string chapterURL)
    {
        LogDebug($"TCB >> PAGES | {chapterURL.AsSpan(10)}");
        var doc = await _web.LoadFromWebAsync(URL_BASE + chapterURL);
        return doc.DocumentNode.SelectNodes(_xp_ChapterPage)
            .Select(x => x.Attributes["src"].Value)
            .ToList();
    }
}