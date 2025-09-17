using System.Text;
using PF_Bot.Core.Editing;
using PF_Bot.Routing.Commands;
using PF_Bot.Telegram;
using PF_Tools.FFMpeg;
using PF_Tools.ProcessRunning;
using Telegram.Bot.Types;

namespace PF_Bot.Core.Internet.Piracy;

// yt-dlp --no-mtime --no-warnings --cookies-from-browser firefox -f ba -k -x --audio-format mp3 -I 1 "URL" -o "%(artist)s - %(title)s xd.%(ext)s"
// yt-dlp --no-mtime --no-warnings --cookies-from-browser firefox -f "bv*[height<=720]" -k -I 1 "URL" -o "video.%(ext)s"

public class DownloadMusicTask(string id, bool youTube, CommandContext context, int messageToDelete)
{
    private const string _YT_video = "https://youtu.be/";
    private const string _YT_list  = "https://www.youtube.com/playlist?list=";

    private static readonly Regex
        _rgx_name  = new(@"(?:NA - )?(?:([\S\s][^-]+) - )?([\S\s]+)? xd\.mp3", RegexOptions.Compiled),
        _rgx_thumb = new(".jpg$|.png$|.webp$", RegexOptions.Compiled);

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
        builder.AppendInQuotes(url).Append(" -o ").AppendInQuotes(output);
        return builder.ToString();
    }

    private string GetVideoArgs(string url)
    {
        var builder = new StringBuilder(YtDlp.DEFAULT_ARGS);
        builder.Append("-f \"bv*[height<=720]\" -k ");
        builder.Append("-I ").Append(PlayListIndex ?? "1").Append(' ');
        builder.AppendInQuotes(url).Append(" -o ").AppendInQuotes("video.%(ext)s");
        return builder.ToString();
    }

    public async Task RunAsync()
    {
        var sw = Stopwatch.StartNew();

        // GET READY

        var url = youTube
            ? PlaylistID?.Length > 0 && (id.Length < 1 || PlayListIndex is not null)
                ? _YT_list + PlaylistID
                : _YT_video + id
            : id;

        var artist = Artist ?? (Uploader ? "%(uploader)s" : "%(artist)s");
        var title  = Title  ??                              "%(title)s";

        var output = $"{artist} - {title} xd.%(ext)s";

        var directory = Dir_Temp
            .Combine($"song-{DateTime.Now.Ticks}")
            .EnsureDirectoryExist();

        var thumbPath = directory.Combine("thumb.jpg");

        // DOWNLOAD AUDIO + THUMB SOURCE

        var taskA = YtDlp.Run(GetAudioArgs(url, output), directory);
        var taskV = ExtractThumb
            ? YtDlp.Run(GetVideoArgs(url), directory)
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
                : GetThumbFile() ?? File_DefaultAlbumCover;

        string GetFile(string pattern)
            => directoryInfo.GetFiles(pattern)[0].FullName;

        string? GetThumbFile()
            => directoryInfo.GetFiles().FirstOrDefault(x => _rgx_thumb.IsMatch(x.FullName))?.FullName;

        // META INFORMATION
        var meta = _rgx_name.Match(Path.GetFileName(audioFile));
        Artist ??= meta.GroupOrNull(1);
        Title  ??= meta.GroupOrNull(2);

        if (NameOnly) Artist = null;
        if (RemoveBrackets) Title = Title?.RemoveTextInBrackets();

        // COMBINE ALL TOGETHER

        var resize = CropSquare || ArtAttached || thumbPath.Value.Contains("maxres");

        var art = directory.Combine("art.jpg");
        var jpg = directory.Combine("jpg.jpg");
        var mp3 = GetSongName(audioFile, Artist, Title!);

        await FFMpeg.Command(thumbSource, art, GetThumbSourceOptions(resize)).FFMpeg_Run();

        var taskMp3 = FFMpeg_AddArtAndMetadata(audioFile, mp3, art, Artist, Title!);
        var taskJpg = FFMpeg_CompressArt(art, jpg); // telegram preview

        await Task.WhenAll(taskMp3, taskJpg);

        // SEND THE RESULT todo move to bot

        Bot.DeleteMessageAsync(context.Chat, messageToDelete);

        await using var stream = File.OpenRead(mp3);
        Bot.SendAudio(context.Origin, InputFile.FromStream(stream, mp3), jpg);
        Log($"{context.Title} >> YOUTUBE MUSIC >> TIME: {sw.ElapsedReadable()}", LogLevel.Info, LogColor.Yellow);
    }

    private string GetSongName(FilePath audioFile, string? artist, string title)
    {
        var artist_or_empty = artist == null
            ? ""
            : $"{artist} - ";
        var name = $"{artist_or_empty}{title}";
        var validName = name.ValidFileName('#');
        return Path.Combine(audioFile.DirectoryName!, $"{validName}.mp3");
    }

    private async Task FFMpeg_AddArtAndMetadata(FilePath audioFile, FilePath output, FilePath art, string? artist, string title)
    {
        var options = FFMpeg.OutputOptions()
            .Map("0:0")
            .Map("1:0")
            .Options("-c copy")
            .Options("-id3v2_version 3")
            .Options("-metadata:s:v title=\"Album cover\"")
            .Options("-metadata:s:v comment=\"Cover (front)\"")
            .Options($"-metadata title=\"{title}\"");
        if (artist != null) options.Options($"-metadata artist=\"{artist}\"");

        await FFMpeg.Command(audioFile, output, options)
            .Input(art)
            .FFMpeg_Run();
    }

    private async Task FFMpeg_CompressArt(FilePath art, FilePath output)
    {
        var probe = await FFProbe.Analyze(art);
        var size = probe.GetVideoStream().Size.FitSize(320);
        var options = FFMpeg.OutputOptions()
            .Options("-qscale:v 7")
            .Resize(size);
        await FFMpeg.Command(art, output, options).FFMpeg_Run();
    }

    private FFMpegOutputOptions GetThumbSourceOptions(bool resize)
    {
        var options = FFMpeg.OutputOptions().Options("-qscale:v 2");

        if (ExtractThumb)
            options.Options("-ss 1 -frames:v 1");

        return ExtractThumb || resize
            ? CropSquare
                ? options.VF("crop='min(iw,ih)':'min(iw,ih)',scale=640:640")
                : options.VF("scale=640:-1")
            : options;
    }
}