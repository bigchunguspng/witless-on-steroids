using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Witlesss.Commands;

namespace Witlesss.Services.Internet;

public class DownloadVideoTask
{
    private bool YouTube;
    private readonly string ID;
    private readonly CommandContext Context;

    private readonly Stopwatch Timer = new();
    
    private static readonly DownloadCache _cache = new(32);

    public DownloadVideoTask(string id, CommandContext context)
    {
        ID = id;
        Context = context;
    }

    private string GetDownloadCommand(string url)
    {
        var builder = new StringBuilder("/C yt-dlp --no-mtime ");
        if (YouTube) builder.Append("-f 18 ");
        builder.Append("-k -I 1 ");
        builder.Append(Quote(url)).Append(" -o ").Append(Quote("video.%(ext)s"));
        return builder.ToString();
    }

    public async Task<string> RunAsync()
    {
        var directory = $"{TEMP_FOLDER}/{DateTime.Now.Ticks}";
        Directory.CreateDirectory(directory);

        if (_cache.Contains(ID, out var path))
        {
            var copy = Path.Combine(directory, Path.GetFileName(path));
            File.Copy(path, copy);
            return copy;
        }

        YouTube = ID.Contains("youtu");

        await DownloadMusicTask.RunCMD(GetDownloadCommand(ID), directory);

        Log($"{Context.Title} >> VIDEO DOWNLOADED >> TIME: {Timer.CheckElapsed()}", ConsoleColor.Blue);

        var result = new DirectoryInfo(directory).GetFiles("video.mp4")[0].FullName;
        _cache.Add(ID, result);

        return result;
    }
}