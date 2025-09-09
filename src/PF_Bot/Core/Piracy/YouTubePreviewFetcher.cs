using System.Net;

#pragma warning disable SYSLIB0014

namespace PF_Bot.Core.Piracy
{
    public static class YouTubePreviewFetcher
    {
        public static Task<FilePath> DownloadPreview(string id, FilePath directory) => Task.Run(() =>
        {
            var path = File_DefaultAlbumCover;
            var urls = new[]
            {
                $"https://i1.ytimg.com/vi_webp/{id}/maxresdefault.webp",
                $"https://i1.ytimg.com/vi_webp/{id}/mqdefault.webp",
                $"https://i1.ytimg.com/vi/{id}/hqdefault.jpg"
            };
            using var client = new WebClient();
            for (var i = 0; i < 3; i++)
            {
                try
                {
                    path = directory.Combine(Path.GetFileName(urls[i]));
                    client.DownloadFile(urls[i], path);
                    break;
                }
                catch
                {
                    if (i == 2) return File_DefaultAlbumCover;
                }
            }

            return path;
        });
    }
}