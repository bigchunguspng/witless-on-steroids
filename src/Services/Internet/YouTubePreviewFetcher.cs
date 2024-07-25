using System.IO;
using System.Net;
using System.Threading.Tasks;

#pragma warning disable SYSLIB0014

namespace Witlesss.Services.Internet
{
    public static class YouTubePreviewFetcher
    {
        public static Task<string> DownloadPreview(string id, string directory) => Task.Run(() =>
        {
            string path = null!;
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
                    path = $"{directory}/{Path.GetFileName(urls[i])}";
                    client.DownloadFile(urls[i], path);
                    break;
                }
                catch
                {
                    if (i == 2) return Paths.File_DefaultAlbumCover;
                }
            }

            return path;
        });
    }
}