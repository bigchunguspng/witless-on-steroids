using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Types.InputFiles;
using Witlesss.Commands;
using Witlesss.MediaTools;
using Stopwatch = Witlesss.Services.Technical.Stopwatch;

#pragma warning disable CS4014

namespace Witlesss.Services.Internet;

public class DownloadMusicTask
{
    private const string _YT_video = "https://youtu.be/";
    private const string _YT_list  = "https://www.youtube.com/playlist?list=";

    private readonly Regex _name = new(@"(?:NA - )?(?:([\S\s][^-]+) - )?([\S\s]+)? xd\.mp3");

    private readonly string ID, PlaylistID, PlayListIndex;
    private readonly string File;
    private readonly int MessageToDelete;
    private readonly bool YouTube;
    private readonly MessageData Message;

    private readonly bool HighQuality, NameOnly, ExtractThumb, RemoveBrackets, Uploader, CropSquare, ArtAttached;

    private readonly Stopwatch Timer = new();

    private int Format;
    
    public string Artist;
    public string Title;

    private Bot Bot => Bot.Instance;

    public DownloadMusicTask(string id, string options, string file, int message, bool yt, string pl, MessageData data, int format = 251)
    {
        ID = id;
        File = file;
        MessageToDelete = message;
        YouTube = yt;
        PlaylistID = pl;
        Message = data;
        Format = format;
        
        HighQuality    = options.Contains('q');
        NameOnly       = options.Contains('n');
        ExtractThumb   = options.Contains('p'); // extract thumbnail from video (default: using YouTube one)
        RemoveBrackets = options.Contains('c');
        Uploader       = options.Contains('u'); // artist is uploader
        CropSquare     = options.Contains('s'); // crop thumbnail to a square

        PlayListIndex  = YouTube && PlaylistID is null ? null : Regex.Match(options, @"\d+").Value;
        if (PlayListIndex?.Length < 1) PlayListIndex = null;

        ArtAttached = File is not null;
        ExtractThumb = ExtractThumb && !ArtAttached;
    }

    private string GetAudioDownloadCommand(string url, string output)
    {
        var builder = new StringBuilder("/C yt-dlp --no-mtime ");
        if (HighQuality)
            builder.Append("--audio-quality 0 ");
        if (YouTube)
            builder.Append("-f ").Append(Format).Append(" -k ");
        else if (!ExtractThumb && !ArtAttached)
            builder.Append("--write-thumbnail ");
        builder.Append("-x --audio-format mp3 ");
        builder.Append("-I ").Append(PlayListIndex ?? "1").Append(' ');
        builder.Append(Quote(url)).Append(" -o ").Append(Quote(output));
        return builder.ToString();
    }

    private string GetVideoDownloadCommand(string url)
    {
        if (!ExtractThumb) return null;
        
        var builder = new StringBuilder("/C yt-dlp --no-mtime ");
        var format = "bv*" + (YouTube ? "[height<=720][filesize<15M]" : "");
        builder.Append("-f ").Append(Quote(format)).Append(" -k ");
        builder.Append("-I ").Append(PlayListIndex ?? "1").Append(' ');
        builder.Append(Quote(url)).Append(" -o ").Append(Quote("video.%(ext)s"));
        return builder.ToString();
    }

    public async Task RunAsync()
    {
        try
        {
            var url = YouTube
                ? PlaylistID.Length > 0 && (ID.Length < 1 || PlayListIndex is not null)
                    ? _YT_list + PlaylistID
                    : _YT_video + ID
                : ID;

            var artist = Artist ?? (Uploader ? "%(uploader)s" : "%(artist)s");
            var title  = Title  ??                              "%(title)s";

            var output = $"{artist} - {title} xd.%(ext)s";

            var cmd_a = GetAudioDownloadCommand(url, output);
            var cmd_v = GetVideoDownloadCommand(url);

            var dir = $"{TEMP_FOLDER}/{DateTime.Now.Ticks}";
            Directory.CreateDirectory(dir);

            var thumb = $"{dir}/thumb.jpg";

            var task_a =                RunCMD(cmd_a, dir);
            var task_v = ExtractThumb ? RunCMD(cmd_v, dir) : ArtAttached ? Bot.DownloadFile(File, thumb, Message.Chat) : YouTube ? GetGoodYouTubeThumbnail() : Task.CompletedTask;
            await Task.WhenAll(task_a, task_v);

            Task GetGoodYouTubeThumbnail() => Task.Run(() => thumb = YouTubePreviewFetcher.DownloadPreview(ID, dir).Result);

            var resize = CropSquare || ArtAttached || thumb.Contains("maxres");

            var directory = new DirectoryInfo(dir);
            var thumb_source = ExtractThumb ? GetFile("video.*") : YouTube || ArtAttached ? thumb : GetFile("*.jpg");
            var audio_file = GetFile("*xd.mp3");

            string GetFile(string pattern) => directory.GetFiles(pattern)[0].FullName;

            var meta = _name.Match(Path.GetFileName(audio_file));
            if (Artist is null && meta.Groups[1].Success) Artist = meta.Groups[1].Value;
            if (Title  is null && meta.Groups[2].Success) Title  = meta.Groups[2].Value;

            if (NameOnly) Artist = null;
            if (RemoveBrackets) Title = Title.RemoveBrackets();

            var img = new F_Process(thumb_source);
            var art = (ExtractThumb ? img.ExportThumbnail(CropSquare) : resize ? img.ResizeThumbnail(CropSquare) : img.CompressJpeg(2)).OutputAs($"{dir}/art.jpg");
            var mp3 = new F_Combine(audio_file, art).AddTrackMetadata(Artist, Title);
            var jpg = new F_Process(art).CompressJpeg(7).OutputAs($"{dir}/jpg.jpg"); // telegram preview

            Task.Run(() => Bot.DeleteMessage(Message.Chat, MessageToDelete));

            await using var stream = System.IO.File.OpenRead(mp3);
            Bot.SendAudio(Message.Chat, new InputOnlineFile(stream, mp3), jpg);
            Log($"{Message.Title} >> YOUTUBE MUSIC >> TIME: {Timer.CheckElapsed()}", ConsoleColor.Yellow);
        }
        catch
        {
            if (Format == 140) throw;

            Format = 140;
            RunAsync();
        }
    }

    private static async Task RunCMD(string cmd, string directory)
    {
        var process = new Process()
        {
            StartInfo = new ProcessStartInfo("cmd.exe", cmd) { WorkingDirectory = directory }
        };
        process.Start();
        await process.WaitForExitAsync();
    }
}