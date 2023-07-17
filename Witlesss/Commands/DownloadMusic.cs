using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using Witlesss.MediaTools;
#pragma warning disable CS4014

namespace Witlesss.Commands
{
    public class DownloadMusic : Command
    {
        private readonly Regex _args = new(@"^\/song\S*\s(http\S*)\s*(?:([\S\s][^-]+) - )?([\S\s]+)?");
        private readonly Regex _name = new(              @"(?:NA - )?(?:([\S\s][^-]+) - )?([\S\s]+)? xd\.mp3");
        private readonly Regex   _id = new(@"(?:(?:\?v=)|(?:v\/)|(?:\.be\/)|(?:embed\/)|(?:u\/1\/))([\w\d-]+)");
        private readonly Regex  _ops = new(@"\/song(\S+)");

        private readonly string _url_prefix = "https://youtu.be/";

        // input: /song[options] URL [artist - ][title]
        public override void Run()
        {
            if (Bot.ThorRagnarok.ChatIsBanned(Chat)) return;

            var text = Text;
            var reply = Message.ReplyToMessage;
            if (reply is { Text: { } t } && !text.Contains("http") && t.StartsWith("http") && !t.Contains(' '))
            {
                var s = Text.Split(' ', 2);
                text = s.Length == 2 ? $"{s[0]} {t} {s[1]}" : $"{s[0]} {t}";
            }

            var cover = GetPhotoFileID(Message) ?? GetPhotoFileID(reply);

            var args = _args.Match(text);

            if (args.Success)
            {
                var url    = args.Groups[1].Value;
                var artist = args.Groups[2].Success ? args.Groups[2].Value : null;
                var title  = args.Groups[3].Success ? args.Groups[3].Value : null;

                var yt = url.Contains("youtu");
                var id = yt ? _id.Match(url).Groups[1].Value : url;
                if (id.Length < 1) throw new Exception("video id was too small");

                var ops = _ops.Match(RemoveBotMention());
                var options = ops.Success ? ops.Groups[1].Value.ToLower() : "";

                var message = Bot.PingChat(Chat, Pick(PLS_WAIT_RESPONSE));

                Bot.RunSafelyAsync(DownloadSongAsync(id, artist, title, options, cover, message, yt, SnapshotMessageData()), Chat, message);
            }
            else
            {
                Bot.SendMessage(Chat, SONG_MANUAL);
            }
        }

        private string GetPhotoFileID(Message message) => message?.Photo is { } p ? p[^1].FileId : null;

        private async Task DownloadSongAsync(string id, string artist, string title, string options, string file, int message, bool yt, CommandParams cp)
        {
            var timer = new StopWatch();

            var hq = options.Contains('q'); // high quality
            var no = options.Contains('n'); // name only
            var xt = options.Contains('p'); // extract thumbnail (from video) (otherwise use youtube one)
            var rb = options.Contains('c'); // remove brackets
            var up = options.Contains('u'); // artist is uploader
            var cs = options.Contains('s'); // crop thumbnail to a square

            var aa = file is not null; // art attached
            if (aa) xt = false;

            var audio = hq ? " --audio-quality 0" : "";

            var url = yt ? _url_prefix + id : id;

            var output = $"{artist ?? (up ? "%(uploader)s" : "%(artist)s")} - {title ?? "%(title)s"} xd.%(ext)s";
            var cmd_a = $"/C yt-dlp --no-mtime {(yt ? "-f 251 -k " : xt || aa ? "" : "--write-thumbnail ")}-x --audio-format mp3{audio} \"{url}\" -o \"{output}\"";
            var cmd_v = xt ? $"/C yt-dlp --no-mtime -f \"bv*{(yt ? "[height<=720][filesize<15M]" : "")}\" -k \"{url}\" -o \"video.%(ext)s\"" : null;

            var dir = $"{TEMP_FOLDER}/{DateTime.Now.Ticks}";
            Directory.CreateDirectory(dir);

            var thumb = $"{dir}/thumb.jpg";
            var task_a =      RunCMD(cmd_a, dir);
            var task_v = xt ? RunCMD(cmd_v, dir) : aa ? Bot.DownloadFile(file, thumb, cp.Chat) : yt ? GetGoodYouTubeThumbnail() : Task.CompletedTask;
            await Task.WhenAll(task_a, task_v);
            
            Task GetGoodYouTubeThumbnail() => Task.Run(() => thumb = YouTubePreviewFetcher.DownloadPreview(id, dir).Result);

            var resize = cs || aa || thumb.Contains("maxres");

            var di = new DirectoryInfo(dir);
            var thumb_source = xt ? di.GetFiles("video.*"  )[0].FullName : yt || aa ? thumb : di.GetFiles("*.jpg")[0].FullName;
            var audio_file   =      di.GetFiles(  "*xd.mp3")[0].FullName;

            var meta = _name.Match(Path.GetFileName(audio_file));
            if (artist is null && meta.Groups[1].Success) artist = meta.Groups[1].Value;
            if (title  is null && meta.Groups[2].Success) title  = meta.Groups[2].Value;

            if (no) artist = null;
            if (rb) title = title.RemoveBrackets();

            var img = $"{dir}/art.jpg";
            var omg = new F_Resize(thumb_source);
            var art = xt ? omg.ExportThumbnail(img, cs) : resize ? omg.ResizeThumbnail(img, cs) : omg.CompressJpeg(img, 2);
            var mp3 = new F_Overlay(audio_file, art).AddTrackMetadata(artist, title);
            var jpg = new F_Resize(art).CompressJpeg($"{dir}/jpg.jpg", 7);

            Task.Run(() => Bot.DeleteMessage(cp.Chat, message));

            await using var stream = File.OpenRead(mp3);
            Bot.SendAudio(cp.Chat, new InputOnlineFile(stream, mp3), jpg);
            Log($"{cp.Title} >> YOUTUBE MUSIC >> TIME: {timer.CheckStopWatch()}", ConsoleColor.Yellow);
        }

        private static async Task RunCMD(string cmd, string directory)
        {
            var info = new ProcessStartInfo("cmd.exe", cmd) { WorkingDirectory = directory };
            var process = new Process() { StartInfo = info };
            process.Start();
            await process.WaitForExitAsync();
        }
    }
}