using System.Text;

namespace Witlesss.Services.Internet;

// yt-dlp --no-mtime --no-warnings --cookies-from-browser firefox -k -I 1 -f "bv*[height<=480][width<=720]+ba/b[height<=480][width<=720]/wv*+ba/w" --remux-video mp4 "URL" -o "video.%(ext)s"

public class DownloadVideoTask(string id, CommandContext context)
{
    private static readonly LimitedCache<string, string> _cache = new(32);

    private string GetDownloadCommand(string url)
    {
        var builder = new StringBuilder(YtDlp.DEFAULT_ARGS);
        var args = "-k -I 1 "
                 + "-f \"bv*[height<=480][width<=720]+ba/b[height<=480][width<=720]/wv*+ba/w\" "
                 + "--remux-video mp4 ";
        builder.Append(args);
        builder.Append(url.Quote()).Append(" -o ").Append("video.%(ext)s".Quote());
        return builder.ToString();
    }

    public async Task<string> RunAsync()
    {
        var sw = GetStartedStopwatch();

        var directory = Path.Combine(Dir_Temp, DateTime.Now.Ticks.ToString());
        Directory.CreateDirectory(directory);

        if (_cache.Contains(id, out var path))
        {
            var newPath = Path.Combine(directory, Path.GetFileName(path));
            File.Copy(path, newPath);
            return newPath;
        }

        await YtDlp.Use(GetDownloadCommand(id), directory);
        Log($"{context.Title} >> VIDEO DOWNLOADED >> TIME: {sw.ElapsedShort()}", ConsoleColor.Blue);

        var result = new DirectoryInfo(directory).GetFiles("video.mp4")[0].FullName;
        _cache.Add(id, result);
        return result;
    }
}