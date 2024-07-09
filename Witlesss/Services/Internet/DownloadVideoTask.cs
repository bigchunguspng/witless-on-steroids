using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Witlesss.Services.Internet;

public class DownloadVideoTask(string id, CommandContext context)
{
    private bool YouTube;

    private readonly Stopwatch Timer = new();
    
    private static readonly LimitedCache<string, string> _cache = new(32);

    private string GetDownloadCommand(string url)
    {
        var builder = new StringBuilder("/C yt-dlp --no-mtime ");
        if (YouTube) builder.Append("-f 18 ");
        builder.Append("-k -I 1 ");
        builder.Append(url.Quote()).Append(" -o ").Append("video.%(ext)s".Quote());
        return builder.ToString();
    }

    public async Task<string> RunAsync()
    {
        var directory = $"{Paths.Dir_Temp}/{DateTime.Now.Ticks}";
        Directory.CreateDirectory(directory);

        if (_cache.Contains(id, out var path))
        {
            var copy = Path.Combine(directory, Path.GetFileName(path));
            File.Copy(path, copy);
            return copy;
        }

        YouTube = id.Contains("youtu");

        await DownloadMusicTask.RunCMD(GetDownloadCommand(id), directory);

        Log($"{context.Title} >> VIDEO DOWNLOADED >> TIME: {Timer.CheckElapsed()}", ConsoleColor.Blue);

        var result = new DirectoryInfo(directory).GetFiles("video.mp4")[0].FullName;
        _cache.Add(id, result);

        return result;
    }
}