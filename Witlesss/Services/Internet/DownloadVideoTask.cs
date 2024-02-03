using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Witlesss.Commands;

namespace Witlesss.Services.Internet;

public class DownloadVideoTask
{
    private readonly string ID;
    private readonly MessageData Message;

    private readonly Stopwatch Timer = new();

    public DownloadVideoTask(string id, MessageData data)
    {
        ID = id;
        Message = data;
    }

    private string GetDownloadCommand(string url)
    {
        var builder = new StringBuilder("/C yt-dlp --no-mtime -f 18 -k -I 1 ");
        builder.Append(Quote(url)).Append(" -o ").Append(Quote("video.%(ext)s"));
        return builder.ToString();
    }

    public async Task<string> RunAsync()
    {
        var cmd = GetDownloadCommand(ID);

        var dir = $"{TEMP_FOLDER}/{DateTime.Now.Ticks}";
        Directory.CreateDirectory(dir);

        await DownloadMusicTask.RunCMD(cmd, dir);

        Log($"{Message.Title} >> VIDEO DOWNLOADED >> TIME: {Timer.CheckElapsed()}", ConsoleColor.Blue);

        return new DirectoryInfo(dir).GetFiles("*.mp4")[0].FullName;
    }
}