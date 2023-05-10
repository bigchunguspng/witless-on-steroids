using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Types.InputFiles;
using Witlesss.MediaTools;
#pragma warning disable CS4014

namespace Witlesss.Commands
{
    public class DownloadMusic : Command
    {
        private readonly Regex _args = new(@"^\/song\S*\s(http\S*)\s*(?:([\S\s]+) - )?([\S\s]+)?");
        private readonly Regex _name = new(              @"(?:NA - )?(?:([\S\s]+) - )?([\S\s]+)? xd\.mp3");
        private readonly Regex   _id = new(@"(?:(?:\?v=)|(?:v\/)|(?:\.be\/)|(?:embed\/)|(?:u\/1\/))([\w\d-]+)");
        private readonly Regex  _ops = new(@"\/song(\S*[qnpc]+)+");

        private readonly string _url_prefix = "https://youtu.be/";
        
        // input: /song(qnpc) URL [artist - ] [title]
        public override void Run()
        {
            if (Bot.ThorRagnarok.ChatIsBanned(Chat)) return;

            var text = Text;
            if (Message.ReplyToMessage is { Text: { } t } && t.StartsWith("http") && !t.Contains(' '))
            {
                var s = Text.Split(' ', 2);
                text = s.Length == 2 ? $"{s[0]} {t} {s[1]}" : $"{s[0]} {t}";
            }

            var args = _args.Match(text);

            if (args.Success)
            {
                var url    = args.Groups[1].Value;
                var artist = args.Groups[2].Success ? args.Groups[2].Value : null;
                var title  = args.Groups[3].Success ? args.Groups[3].Value : null;

                var id = _url_prefix + _id.Match(url).Groups[1].Value;

                var ops = _ops.Match(RemoveBotMention());
                var options = ops.Success ? ops.Groups[1].Value.ToLower() : "";

                RunSafelyAsync(DownloadSongAsync(id, artist, title, options, SnapshotMessageData()));
            }
            else
            {
                Bot.SendMessage(Chat, "*SONG MANUAL*");
            }
        }

        private async Task DownloadSongAsync(string url, string artist, string title, string options, CommandParams cp)
        {
            var hq = options.Contains('q'); // high quality
            var no = options.Contains('n'); // name only
            var xt = options.Contains('p'); // extract thumbnail (from video) (otherwise use youtube one)
            var rb = options.Contains('c'); // remove brackets

            var audio = hq ? " --audio-quality 0" : "";
            var thumb = xt ? "" : " --write-thumbnail";

            var cmd_a = $"/C yt-dlp --no-mtime -f 251 -k -x --audio-format mp3{audio}{thumb} {url} -o \"{artist ?? "%(artist)s"} - {title ?? "%(title)s"} xd\"";
            var cmd_v = xt ? $"/C yt-dlp --no-mtime -f \"bv*[height<=720][filesize<15M]\" -k {url} -o \"video xd.%(ext)s\"" : null;

            var dir = $"{TEMP_FOLDER}/{DateTime.Now.Ticks}";
            Directory.CreateDirectory(dir);

            await         RunCMD(cmd_a, dir);
            if (xt) await RunCMD(cmd_v, dir);

            var di = new DirectoryInfo(dir);
            var vid = di.GetFiles(xt ? "video xd.*" : "*xd.webp")[0].FullName; // video : thumb itself
            var mp3 = di.GetFiles(                    "*xd.mp3" )[0].FullName;

            var meta = _name.Match(Path.GetFileName(mp3));
            if (artist is null && meta.Groups[1].Success) artist = meta.Groups[1].Value;
            if (title  is null && meta.Groups[2].Success) title  = meta.Groups[2].Value;

            var ffmpeg = new F_Resize(vid);
            var art = xt ? ffmpeg.ExportThumbnail() : ffmpeg.Transcode(".png");
            var track = new F_Overlay(mp3, art).AddTrackMetadata(artist, title);
            var jpg = new F_Resize(xt ? art : track).Transcode(".jpg");

            await using var stream = File.OpenRead(track);
            Bot.SendAudio(cp.Chat, new InputOnlineFile(stream, track), jpg);
            Log($"{cp.Title} >> SONG [mp3]");
        }

        private static async Task RunCMD(string cmd, string directory)
        {
            var info = new ProcessStartInfo("cmd.exe", cmd) { WorkingDirectory = directory };
            var process = new Process() { StartInfo = info };
            process.Start();
            await RunSafelyAsync(process.WaitForExitAsync());
        }

        private static async Task RunSafelyAsync(Task task)
        {
            try
            {
                await task.ContinueWith(action =>
                {
                    LogError($"BRUH -> {FixedErrorMessage(action.Exception?.Message)}");
                }, TaskContinuationOptions.NotOnRanToCompletion);
            }
            catch
            {
                // xd
            }
        }

        // yt-dlp --no-mtime -k -x --audio-format mp3 [--audio-quality 0] [--write-thumbnail] https://youtu.be/ZGSt0b0pAHw -o "%(artist)s - %(title)s xd"

        // yt-dlp --no-mtime -f 251 -k -x --audio-format mp3 [--audio-quality 0] [--write-thumbnail] https://youtu.be/ZGSt0b0pAHw -o "%(artist)s - %(title)s xd"
        // yt-dlp --no-mtime -f b -k https://youtu.be/ZGSt0b0pAHw -o "%(artist)s - %(title)s xd.%(ext)s"

        // ffmpeg -i "MONOGATARI Series - Hourousha xd.webm" -frames:v 1 -vf scale=-1:640 art3.png
        // ffmpeg -i "MONOGATARI Series - Hourousha xd.mp3" -i art3.png -map 0:0 -map 1:0 -c copy -id3v2_version 3 -metadata:s:v title="Album cover" -metadata:s:v comment="Cover (front)" -metadata artist="MONOGATARI OST" -metadata title="Hourousha" out_md.mp3
    }
}