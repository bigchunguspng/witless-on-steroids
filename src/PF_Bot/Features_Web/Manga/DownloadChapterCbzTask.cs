using System.IO.Compression;

namespace PF_Bot.Features_Web.Manga;

public class DownloadChapterCbzTask
(
    List<string> links,
    string title,
    string chapter
)
{
    private readonly FilePath
        Dir_Title   = Dir_Manga
            .Combine(title),
        Dir_Chapter = Dir_Manga
            .Combine(title)
            .Combine(chapter)
            .EnsureDirectoryExist();

    public async Task<FilePath> Run()
    {
        var archive = Dir_Title.Combine($"{title}-{chapter}.cbz");
        if (archive.FileExists.Janai())
        {
            var sw = Stopwatch.StartNew();

            LogDebug($"CBZ >> DOWNLOADING | {links.Count} pages");
            await DownloadPages();

            LogDebug("CBZ >> ARCHIVING");
            AddPagesToArchive(archive);

            LogDebug($"CBZ >> DONE >> {sw.ElapsedReadable()}", LogColor.Lime);
        }
        else
            LogDebug("CBZ >> CHAPTER EXIST");

        return archive;
    }

    private async Task DownloadPages()
    {
        var page = 0;

        using var client = HttpClientFactory.CreateClient();
        foreach (var link in links)
        {
            var name = $"{chapter} - {++page:000}{Path.GetExtension(link)}";
            var path = Dir_Chapter.Combine(name);

            await client.DownloadFileAsync(link, path);
        }
    }

    private void AddPagesToArchive(string archive)
    {
        var pages = Dir_Chapter.GetFiles($"{chapter} - *.*");

        using var zip = ZipFile.Open(archive, ZipArchiveMode.Update);
        foreach (var page in pages)
        {
            var name = Path.GetFileName(page);
            zip.CreateEntryFromFile(page, name);
        }
    }
}