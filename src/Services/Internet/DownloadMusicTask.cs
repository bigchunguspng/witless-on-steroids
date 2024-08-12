using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Types.InputFiles;
using Witlesss.Backrooms.Helpers;
using Witlesss.MediaTools;
using Stopwatch = Witlesss.Services.Technical.Stopwatch;

namespace Witlesss.Services.Internet;

public class DownloadMusicTask(string id, bool youTube, CommandContext context, int messageToDelete)
{
    private const string _YT_video = "https://youtu.be/";
    private const string _YT_list  = "https://www.youtube.com/playlist?list=";

    private static readonly Regex _name = new(@"(?:NA - )?(?:([\S\s][^-]+) - )?([\S\s]+)? xd\.mp3");

    public required string? PlaylistID;
    public required string? PlayListIndex;
    public required string? Cover;

    private bool ArtAttached => Cover is not null;

    public required bool HighQuality;
    public required bool NameOnly;
    public required bool ExtractThumb;      // extract thumbnail from video (default: using YouTube one)
    public required bool RemoveBrackets;
    public required bool Uploader;          // artist is uploader
    public required bool CropSquare;        // crop thumbnail to a square

    private readonly Stopwatch Timer = new();

    public string? Artist;
    public string? Title;

    private Bot Bot => Bot.Instance;

    private string GetAudioArgs(string url, string output)
    {
        var builder = new StringBuilder(YtDlp.DEFAULT_ARGS);
        if (HighQuality)
            builder.Append("--audio-quality 0 ");
        if (!youTube && !ExtractThumb && !ArtAttached)
            builder.Append("--write-thumbnail ");
        builder.Append("-f ba -k -x --audio-format mp3 ");
        builder.Append("-I ").Append(PlayListIndex ?? "1").Append(' ');
        builder.Append(url.Quote()).Append(" -o ").Append(output.Quote());
        return builder.ToString();
    }

    private string GetVideoArgs(string url)
    {
        var builder = new StringBuilder(YtDlp.DEFAULT_ARGS);
        builder.Append("-f \"bv*[height<=720]\" -k ");
        builder.Append("-I ").Append(PlayListIndex ?? "1").Append(' ');
        builder.Append(url.Quote()).Append(" -o ").Append("video.%(ext)s".Quote());
        return builder.ToString();
    }

    public async Task RunAsync()
    {
        // GET READY

        var url = youTube
            ? PlaylistID?.Length > 0 && (id.Length < 1 || PlayListIndex is not null)
                ? _YT_list + PlaylistID
                : _YT_video + id
            : id;

        var artist = Artist ?? (Uploader ? "%(uploader)s" : "%(artist)s");
        var title  = Title  ??                              "%(title)s";

        var output = $"{artist} - {title} xd.%(ext)s";

        var directory = $"{Dir_Temp}/{DateTime.Now.Ticks}";
        var thumbPath = $"{directory}/thumb.jpg";

        Directory.CreateDirectory(directory);

        // DOWNLOAD AUDIO + THUMB SOURCE

        var taskA = YtDlp.Use(GetAudioArgs(url, output), directory);
        var taskV = ExtractThumb
            ? YtDlp.Use(GetVideoArgs(url), directory)
            : ArtAttached
                ? Bot.DownloadFile(Cover!, thumbPath, context.Chat)
                : youTube
                    ? Task.Run(() => thumbPath = YouTubePreviewFetcher.DownloadPreview(id, directory).Result)
                    : Task.CompletedTask;
        await Task.WhenAll(taskA, taskV);

        // GRAB FILES

        var directoryInfo = new DirectoryInfo(directory);
        var audioFile = GetFile("*xd.mp3");
        var thumbSource = ExtractThumb
            ? GetFile("video.*")
            : youTube || ArtAttached
                ? thumbPath
                : GetFile("*.jpg");

        string GetFile(string pattern) => directoryInfo.GetFiles(pattern)[0].FullName;

        // META INFORMATION
        var meta = _name.Match(Path.GetFileName(audioFile));
        if (Artist is null && meta.Groups[1].Success) Artist = meta.Groups[1].Value;
        if (Title  is null && meta.Groups[2].Success) Title  = meta.Groups[2].Value;

        if (NameOnly) Artist = null;
        if (RemoveBrackets) Title = Title?.RemoveBrackets();

        // COMBINE ALL TOGETHER

        var resize = CropSquare || ArtAttached || thumbPath.Contains("maxres");

        var img = new F_Process(thumbSource);
        var imgProcess = ExtractThumb
            ? img.ExportThumbnail(CropSquare)
            : resize
                ? img.ResizeThumbnail(CropSquare)
                : img.CompressJpeg(2);
        var art = await imgProcess.OutputAs($"{directory}/art.jpg");
        var mp3 = new F_Combine(audioFile, art).AddTrackMetadata(Artist, Title!);
        var jpg = new F_Process(art).MakeThumb(7).OutputAs($"{directory}/jpg.jpg"); // telegram preview

        await Task.WhenAll(mp3, jpg);

        // SEND THE RESULT

        Bot.DeleteMessageAsync(context.Chat, messageToDelete);

        await using var stream = File.OpenRead(mp3.Result);
        Bot.SendAudio(context.Chat, new InputOnlineFile(stream, mp3.Result), jpg.Result);
        Log($"{context.Title} >> YOUTUBE MUSIC >> TIME: {Timer.CheckElapsed()}", ConsoleColor.Yellow);
    }
}