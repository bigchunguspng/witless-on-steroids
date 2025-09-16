using System.Text;
using PF_Bot.Routing.Commands;
using PF_Tools.FFMpeg;
using PF_Tools.ProcessRunning;

namespace PF_Bot.Core.Internet.Piracy;

// yt-dlp --no-mtime --no-warnings --cookies-from-browser firefox -k -I 1 -f "bv*[height<=480][width<=720][vcodec*=avc]+ba[acodec*=mp4a]/b[height<=480][width<=720][vcodec*=avc][acodec*=mp4a]/bv*[height<=480][width<=720]+ba/b[height<=480][width<=720]/wv*+ba/w" --remux-video gif>gif/mp4 "URL" -o "video.%(ext)s"

public class DownloadVideoTask(string id, CommandContext context)
{
    private static readonly LimitedCache<string, string> _cache = new(32);

    private string GetDownloadCommand(string url)
    {
        var builder = new StringBuilder(YtDlp.DEFAULT_ARGS);
        var args = "-k -I 1 "
                 + "-f \""
                 + "bv*[height<=480][width<=720][vcodec*=avc]+ba[acodec*=mp4a]/"
                 +   "b[height<=480][width<=720][vcodec*=avc][acodec*=mp4a]/"
                 + "bv*[height<=480][width<=720]+ba/"
                 +   "b[height<=480][width<=720]/"
                 + "wv*+ba/w\" "
                 + "--remux-video gif>gif/mp4 ";
        builder.Append(args);
        builder.AppendInQuotes(url).Append(" -o ").AppendInQuotes("video.%(ext)s");
        return builder.ToString();
    }

    public async Task<string> RunAsync()
    {
        var sw = Stopwatch.StartNew();

        var directory = Path.Combine(Dir_Temp, $"vid-{DateTime.Now.Ticks}");
        Directory.CreateDirectory(directory);

        if (_cache.Contains(id, out var path))
        {
            var newPath = Path.Combine(directory, Path.GetFileName(path));
            File.Copy(path, newPath);
            return newPath;
        }

        await YtDlp.Run(GetDownloadCommand(id), directory);
        Log($"{context.Title} >> VIDEO DOWNLOADED >> TIME: {sw.ElapsedReadable()}", LogLevel.Info, LogColor.Yellow);

        var directoryInfo = new DirectoryInfo(directory);
        var files = directoryInfo.GetFiles("video.mp4");
        var result = files.Length > 0
            ? files[0].FullName
            : await FFMpeg_GifToMp4(GetVideoFile(directoryInfo)); // in case of failed to remux GIF
        _cache.Add(id, result);
        return result;
    }

    private string GetVideoFile
        (DirectoryInfo directoryInfo) => directoryInfo
        .GetFiles("video.*")
        .OrderByDescending(x => x.Length)
        .First().FullName;

    private async Task<string> FFMpeg_GifToMp4(FilePath input)
    {
        var output = input.ChangeExtension(".mp4");
        await FFMpeg.Command(input, output, "").FFMpeg_Run();
        return output;
    }
}