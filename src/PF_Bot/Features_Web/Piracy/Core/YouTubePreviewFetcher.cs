namespace PF_Bot.Features_Web.Piracy.Core;

public static class YouTubePreviewFetcher
{
    public static async Task<FilePath> DownloadPreview(string id, FilePath directory)
    {
        YTPD_Log(ConsoleColor.Gray);

        var urls = new[]
        {
            $"https://i1.ytimg.com/vi_webp/{id}/maxresdefault.webp",
            $"https://i1.ytimg.com/vi_webp/{id}/mqdefault.webp",
            $"https://i1.ytimg.com/vi/{id}/hqdefault.jpg",
        };

        using var client = new HttpClient();

        foreach (var url in urls)
        {
            var type = Path.GetFileNameWithoutExtension(url);
            var ext  = Path.GetExtension(url);
            var path = directory.Combine($"{type}-{id}{ext}");
            try
            {
                await client.DownloadFileAsync(url, path);

                YTPD_Log(ConsoleColor.Green, $"-{type} >> DONE");

                return path;
            }
            catch
            {
                YTPD_Log(ConsoleColor.Red, $"-{type} >> FAIL");
            }
        }

        YTPD_Log(ConsoleColor.Yellow, " >> DEFAULT");

        return File_DefaultAlbumCover;

        void YTPD_Log
            (ConsoleColor color, string? text = null)
            => Print($"YTPD | {id}{text}", color);
    }
}