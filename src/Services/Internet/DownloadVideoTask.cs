using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Witlesss.Backrooms.Helpers;

namespace Witlesss.Services.Internet;

public class DownloadVideoTask(string id, CommandContext context)
{
    private readonly Stopwatch Timer = new(); // todo -> sw
    
    private static readonly LimitedCache<string, string> _cache = new(32);

    private string GetDownloadCommand(string url)
    {
        var builder = new StringBuilder(YtDlp.DEFAULT_ARGS);
        builder.Append("-k -I 1 -f \"bv*[height<=480]+ba/b[height<=480]/wv*+ba/w\" --remux-video mp4");
        builder.Append(url.Quote()).Append(" -o ").Append("video.%(ext)s".Quote());
        return builder.ToString();
    }

    public async Task<string> RunAsync()
    {
        var directory = Path.Combine(Paths.Dir_Temp, DateTime.Now.Ticks.ToString());
        Directory.CreateDirectory(directory);

        if (_cache.Contains(id, out var path))
        {
            var newPath = Path.Combine(directory, Path.GetFileName(path));
            File.Copy(path, newPath);
            return newPath;
        }

        await YtDlp.Use(GetDownloadCommand(id), directory);
        Log($"{context.Title} >> VIDEO DOWNLOADED >> TIME: {Timer.CheckElapsed()}", ConsoleColor.Blue);

        var result = new DirectoryInfo(directory).GetFiles("video.mp4")[0].FullName;
        _cache.Add(id, result);
        return result;
    }
}