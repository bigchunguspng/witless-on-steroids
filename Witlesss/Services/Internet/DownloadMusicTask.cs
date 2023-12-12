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
    private const string _YT_url_prefix = "https://youtu.be/";
    private readonly Regex _name = new(@"(?:NA - )?(?:([\S\s][^-]+) - )?([\S\s]+)? xd\.mp3");

    private readonly string ID;
    private readonly string File;
    private readonly int Message;
    private readonly bool YouTube;
    private readonly CommandParams Cp;

    private readonly bool HighQuality, NameOnly, ExtractThumb, RemoveBrackets, Uploader, CropSquare, ArtAttached;

    private readonly Stopwatch Timer = new();

    private int Format;
    
    public string Artist;
    public string Title;

    private Bot Bot => Command.Bot;

    public DownloadMusicTask(string id, string options, string file, int message, bool yt, CommandParams cp, int format = 251)
    {
        ID = id;
        File = file;
        Message = message;
        YouTube = yt;
        Cp = cp;
        Format = format;
        
        HighQuality    = options.Contains('q');
        NameOnly       = options.Contains('n');
        ExtractThumb   = options.Contains('p'); // extract thumbnail from video (default: using YouTube one)
        RemoveBrackets = options.Contains('c');
        Uploader       = options.Contains('u'); // artist is uploader
        CropSquare     = options.Contains('s'); // crop thumbnail to a square
        
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
        builder.Append(Quote(url)).Append(" -o ").Append(Quote(output));
        return builder.ToString();
    }

    private string GetVideoDownloadCommand(string url)
    {
        if (!ExtractThumb) return null;
        
        var builder = new StringBuilder("/C yt-dlp --no-mtime ");
        var format = "bv*" + (YouTube ? "[height<=720][filesize<15M]" : "");
        builder.Append("-f ").Append(Quote(format)).Append(" -k ");
        builder.Append(Quote(url)).Append(" -o ").Append(Quote("video.%(ext)s"));
        return builder.ToString();
    }

    public async Task RunAsync()
    {
        try
        {
            var url = YouTube ? _YT_url_prefix + ID : ID;

            var artist = Artist ?? (Uploader ? "%(uploader)s" : "%(artist)s");
            var title  = Title  ??                              "%(title)s";

            var output = $"{artist} - {title} xd.%(ext)s";

            var cmd_a = GetAudioDownloadCommand(url, output);
            var cmd_v = GetVideoDownloadCommand(url);

            var dir = $"{TEMP_FOLDER}/{DateTime.Now.Ticks}";
            Directory.CreateDirectory(dir);

            var thumb = $"{dir}/thumb.jpg";

            var task_a =                RunCMD(cmd_a, dir);
            var task_v = ExtractThumb ? RunCMD(cmd_v, dir) : ArtAttached ? Bot.DownloadFile(File, thumb, Cp.Chat) : YouTube ? GetGoodYouTubeThumbnail() : Task.CompletedTask;
            await Task.WhenAll(task_a, task_v);

            Task GetGoodYouTubeThumbnail() => Task.Run(() => thumb = YouTubePreviewFetcher.DownloadPreview(ID, dir).Result);

            var resize = CropSquare || ArtAttached || thumb.Contains("maxres");

            var di = new DirectoryInfo(dir);
            var thumb_source = ExtractThumb ? di.GetFiles("video.*")[0].FullName : YouTube || ArtAttached ? thumb : di.GetFiles("*.jpg")[0].FullName;
            var audio_file = di.GetFiles("*xd.mp3")[0].FullName;

            var meta = _name.Match(Path.GetFileName(audio_file));
            if (Artist is null && meta.Groups[1].Success) Artist = meta.Groups[1].Value;
            if (Title  is null && meta.Groups[2].Success) Title  = meta.Groups[2].Value;

            if (NameOnly) Artist = null;
            if (RemoveBrackets) Title = Title.RemoveBrackets();

            var img = new F_Process(thumb_source);
            var art = (ExtractThumb ? img.ExportThumbnail(CropSquare) : resize ? img.ResizeThumbnail(CropSquare) : img.CompressJpeg(2)).OutputAs($"{dir}/art.jpg");
            var mp3 = new F_Combine(audio_file, art).AddTrackMetadata(Artist, Title);
            var jpg = new F_Process(art).CompressJpeg(7).OutputAs($"{dir}/jpg.jpg"); // telegram preview

            Task.Run(() => Bot.DeleteMessage(Cp.Chat, Message));

            await using var stream = System.IO.File.OpenRead(mp3);
            Bot.SendAudio(Cp.Chat, new InputOnlineFile(stream, mp3), jpg);
            Log($"{Cp.Title} >> YOUTUBE MUSIC >> TIME: {Timer.CheckElapsed()}", ConsoleColor.Yellow);
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