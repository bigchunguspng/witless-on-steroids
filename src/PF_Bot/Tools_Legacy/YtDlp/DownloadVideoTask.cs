using System.Text;
using PF_Bot.Backrooms.Types;
using PF_Bot.Routing.Commands;

namespace PF_Bot.Tools_Legacy.YtDlp;

// yt-dlp --no-mtime --no-warnings --cookies-from-browser firefox -k -I 1 -f "bv*[height<=480][width<=720][vcodec*=avc]+ba[acodec*=mp4a]/b[height<=480][width<=720][vcodec*=avc][acodec*=mp4a]/bv*[height<=480][width<=720]+ba/b[height<=480][width<=720]/wv*+ba/w" --remux-video gif>gif/mp4 "URL" -o "video.%(ext)s"

public class DownloadVideoTask(string id, CommandContext context)
{
    private static readonly LimitedCache<string, string> _cache = new(32);

    private string GetDownloadCommand(string url)
    {
        var builder = new StringBuilder(PF_Tools.YtDlp.YtDlp.DEFAULT_ARGS);
        var args = "-k -I 1 "
                 + "-f \""
                 + "bv*[height<=480][width<=720][vcodec*=avc]+ba[acodec*=mp4a]/"
                 +   "b[height<=480][width<=720][vcodec*=avc][acodec*=mp4a]/"
                 + "bv*[height<=480][width<=720]+ba/"
                 +   "b[height<=480][width<=720]/"
                 + "wv*+ba/w\" "
                 + "--remux-video gif>gif/mp4 ";
        builder.Append(args);
        builder.Append(url.Quote()).Append(" -o ").Append("video.%(ext)s".Quote());
        return builder.ToString();
    }

    public async Task<string> RunAsync()
    {
        var sw = GetStartedStopwatch();

        var directory = Path.Combine(Dir_Temp, $"vid-{DateTime.Now.Ticks}");
        Directory.CreateDirectory(directory);

        if (_cache.Contains(id, out var path))
        {
            var newPath = Path.Combine(directory, Path.GetFileName(path));
            File.Copy(path, newPath);
            return newPath;
        }

        await PF_Tools.YtDlp.YtDlp.Run(GetDownloadCommand(id), directory);
        Log($"{context.Title} >> VIDEO DOWNLOADED >> TIME: {sw.ElapsedShort()}", LogLevel.Info, LogColor.Yellow);

        var directoryInfo = new DirectoryInfo(directory);
        var files = directoryInfo.GetFiles("video.mp4");
        var result = files.Length > 0
            ? files[0].FullName
            : await directoryInfo.GetFiles("video.*") // in case of failed to remux GIF
                .OrderByDescending(x => x.Length)
                .First().FullName
                .UseFFMpeg(context.Origin).Out();
        _cache.Add(id, result);
        return result;
    }
}