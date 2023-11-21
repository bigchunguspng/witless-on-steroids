using System;
using System.Diagnostics;
using System.IO;
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
    private const string _url_prefix = "https://youtu.be/";
    private readonly Regex _name = new(@"(?:NA - )?(?:([\S\s][^-]+) - )?([\S\s]+)? xd\.mp3");
    
    private readonly string ID;
    private readonly string Options;
    private readonly string File;
    private readonly int Message;
    private readonly bool YouTube;
    private readonly CommandParams Cp;
    
    private int Format;
    
    public string Artist;
    public string Title;

    private Bot Bot => Command.Bot;

    public DownloadMusicTask(string id, string options, string file, int message, bool yt, CommandParams cp, int format = 251)
    {
        ID = id;
        Options = options;
        File = file;
        Message = message;
        YouTube = yt;
        Cp = cp;
        Format = format;
    }

    public async Task RunAsync()
    {
        try
        {
            var timer = new Stopwatch();

            var hq = Options.Contains('q'); // high quality
            var no = Options.Contains('n'); // name only
            var xt = Options.Contains('p'); // extract thumbnail from video (otherwise use youtube one)
            var rb = Options.Contains('c'); // remove brackets
            var up = Options.Contains('u'); // artist is uploader
            var cs = Options.Contains('s'); // crop thumbnail to a square

            var aa = File is not null; // art attached
            if (aa) xt = false;

            var audio = hq ? " --audio-quality 0" : "";

            var url = YouTube ? _url_prefix + ID : ID;

            var output = $"{Artist ?? (up ? "%(uploader)s" : "%(artist)s")} - {Title ?? "%(title)s"} xd.%(ext)s";
            var cmd_a = $"/C yt-dlp --no-mtime {(YouTube ? $"-f {Format} -k " : xt || aa ? "" : "--write-thumbnail ")}-x --audio-format mp3{audio} \"{url}\" -o \"{output}\"";
            var cmd_v = xt ? $"/C yt-dlp --no-mtime -f \"bv*{(YouTube ? "[height<=720][filesize<15M]" : "")}\" -k \"{url}\" -o \"video.%(ext)s\"" : null;

            var dir = $"{TEMP_FOLDER}/{DateTime.Now.Ticks}";
            Directory.CreateDirectory(dir);

            var thumb = $"{dir}/thumb.jpg";
            var task_a =      RunCMD(cmd_a, dir);
            var task_v = xt ? RunCMD(cmd_v, dir) : aa ? Bot.DownloadFile(File, thumb, Cp.Chat) : YouTube ? GetGoodYouTubeThumbnail() : Task.CompletedTask;
            await Task.WhenAll(task_a, task_v);
        
            Task GetGoodYouTubeThumbnail() => Task.Run(() => thumb = YouTubePreviewFetcher.DownloadPreview(ID, dir).Result);

            var resize = cs || aa || thumb.Contains("maxres");

            var di = new DirectoryInfo(dir);
            var thumb_source = xt ? di.GetFiles("video.*"  )[0].FullName : YouTube || aa ? thumb : di.GetFiles("*.jpg")[0].FullName;
            var audio_file   =      di.GetFiles(  "*xd.mp3")[0].FullName;

            var meta = _name.Match(Path.GetFileName(audio_file));
            if (Artist is null && meta.Groups[1].Success) Artist = meta.Groups[1].Value;
            if (Title  is null && meta.Groups[2].Success) Title  = meta.Groups[2].Value;

            if (no) Artist = null;
            if (rb) Title = Title.RemoveBrackets();

            var img = $"{dir}/art.jpg";
            var omg = new F_Resize(thumb_source);
            var art = xt ? omg.ExportThumbnail(img, cs) : resize ? omg.ResizeThumbnail(img, cs) : omg.CompressJpeg(img, 2);
            var mp3 = new F_Overlay(audio_file, art).AddTrackMetadata(Artist, Title);
            var jpg = new F_Resize(art).CompressJpeg($"{dir}/jpg.jpg", 7);

            Task.Run(() => Bot.DeleteMessage(Cp.Chat, Message));

            await using var stream = System.IO.File.OpenRead(mp3);
            Bot.SendAudio(Cp.Chat, new InputOnlineFile(stream, mp3), jpg);
            Log($"{Cp.Title} >> YOUTUBE MUSIC >> TIME: {timer.CheckElapsed()}", ConsoleColor.Yellow);
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
        var info = new ProcessStartInfo("cmd.exe", cmd) { WorkingDirectory = directory };
        var process = new Process() { StartInfo = info };
        process.Start();
        await process.WaitForExitAsync();
    }
}