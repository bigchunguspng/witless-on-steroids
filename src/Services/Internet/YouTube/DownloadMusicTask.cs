using System.Text;
using Telegram.Bot.Types;

namespace Witlesss.Services.Internet.YouTube;

// yt-dlp --no-mtime --no-warnings --cookies-from-browser firefox -f ba -k -x --audio-format mp3 -I 1 "URL" -o "%(artist)s - %(title)s xd.%(ext)s"
// yt-dlp --no-mtime --no-warnings --cookies-from-browser firefox -f "bv*[height<=720]" -k -I 1 "URL" -o "video.%(ext)s"

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
        var sw = GetStartedStopwatch();

        // GET READY

        var url = youTube
            ? PlaylistID?.Length > 0 && (id.Length < 1 || PlayListIndex is not null)
                ? _YT_list + PlaylistID
                : _YT_video + id
            : id;

        var artist = Artist ?? (Uploader ? "%(uploader)s" : "%(artist)s");
        var title  = Title  ??                              "%(title)s";

        var output = $"{artist} - {title} xd.%(ext)s";

        var directory = Path.Combine(Dir_Temp, $"song-{DateTime.Now.Ticks}");
        var thumbPath = Path.Combine(directory, "thumb.jpg");

        Directory.CreateDirectory(directory);

        // DOWNLOAD AUDIO + THUMB SOURCE

        var taskA = YtDlp.Use(GetAudioArgs(url, output), directory, context.Origin);
        var taskV = ExtractThumb
            ? YtDlp.Use(GetVideoArgs(url), directory, context.Origin)
            : ArtAttached
                ? Bot.DownloadFile(Cover!, thumbPath, context.Origin)
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
        Artist ??= meta.GroupOrNull(1);
        Title  ??= meta.GroupOrNull(2);

        if (NameOnly) Artist = null;
        if (RemoveBrackets) Title = Title?.RemoveTextInBrackets();

        // COMBINE ALL TOGETHER

        var resize = CropSquare || ArtAttached || thumbPath.Contains("maxres");

        var img = thumbSource.UseFFMpeg(context.Origin);
        var imgProcess = ExtractThumb
            ? img.ExportThumbnail(CropSquare)
            : resize
                ? img.ResizeThumbnail(CropSquare)
                : img.CompressJpeg(2);
        var art = await imgProcess.OutAs($"{directory}/art.jpg");
        var mp3 = audioFile.UseFFMpeg(context.Origin).AddTrackMetadata(art, Artist, Title!);
        var jpg = art      .UseFFMpeg(context.Origin).MakeThumb(7).OutAs($"{directory}/jpg.jpg"); // telegram preview

        await Task.WhenAll(mp3, jpg);

        // SEND THE RESULT

        Bot.DeleteMessageAsync(context.Chat, messageToDelete);

        await using var stream = File.OpenRead(mp3.Result);
        Bot.SendAudio(context.Origin, InputFile.FromStream(stream, mp3.Result), jpg.Result);
        Log($"{context.Title} >> YOUTUBE MUSIC >> TIME: {sw.ElapsedShort()}", LogLevel.Info, 11);
    }
}