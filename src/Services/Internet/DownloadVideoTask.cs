using System.Text;
using Witlesss.Backrooms.Types;

namespace Witlesss.Services.Internet;

// yt-dlp --no-mtime --no-warnings --cookies-from-browser firefox -k -I 1 -f "bv*[height<=480][width<=720]+ba/b[height<=480][width<=720]/wv*+ba/w" --remux-video gif>gif/mp4 "URL" -o "video.%(ext)s"

public class DownloadVideoTask(string id, CommandContext context)
{
    private static readonly LimitedCache<string, string> _cache = new(32);

    private string GetDownloadCommand(string url)
    {
        var builder = new StringBuilder(YtDlp.DEFAULT_ARGS);
        var args = "-k -I 1 "
                 + "-f \"bv*[height<=480][width<=720]+ba/b[height<=480][width<=720]/wv*+ba/w\" "
                 + "--remux-video gif>gif/mp4 ";
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

        await YtDlp.Use(GetDownloadCommand(id), directory, context.Chat);
        Log($"{context.Title} >> VIDEO DOWNLOADED >> TIME: {sw.ElapsedShort()}", LogLevel.Info, 11);

        var directoryInfo = new DirectoryInfo(directory);
        var files = directoryInfo.GetFiles("video.mp4");
        var result = files.Length > 0
            ? files[0].FullName
            : await directoryInfo.GetFiles("video.*") // in case of failed to remux GIF
                .OrderByDescending(x => x.Length)
                .First().FullName
                .UseFFMpeg(context.Chat).Out();
        _cache.Add(id, result);
        return result;
    }
}