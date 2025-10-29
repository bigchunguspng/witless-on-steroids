using System.IO.Compression;
using PF_Tools.FFMpeg;

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
            .Combine(chapter),
        Dir_JPG     = Dir_Manga
            .Combine(title)
            .Combine(chapter)
            .Combine("JPG")
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
        var paths = links.Select((link, i) =>
        {
            var page = 1 + i;
            var name = $"{chapter} - {page:000}";
            var path1 = Dir_Chapter.Combine($"{name}{Path.GetExtension(link)}");
            var path2 = Dir_JPG    .Combine($"{name}.jpg");
            return (URL: link, Download: path1, Compress: path2);
        });

        using var client = HttpClientFactory.CreateClient();
        var tasks = paths.Select(async x => 
        {
            await client.DownloadFileAsync(x.URL, x.Download);
            await FFMpeg.Command(x.Download, x.Compress, "-q 5").FFMpeg_Run();
        });
        await Task.WhenAll(tasks);
    }

    private void AddPagesToArchive(string archive)
    {
        var pages = Dir_JPG.GetFiles($"{chapter} - *.*");

        using var zip = ZipFile.Open(archive, ZipArchiveMode.Update);
        foreach (var page in pages)
        {
            var name = Path.GetFileName(page);
            zip.CreateEntryFromFile(page, name);
        }
    }
}