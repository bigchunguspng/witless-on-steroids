using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Types.InputFiles;
using Witlesss.MediaTools;

namespace Witlesss.Commands
{
    public class DownloadMusic : Command
    {
        private readonly Regex _args = new(@"^\/song\S*\s(http\S*)\s*(?:([\S\s]+) - )?([\S\s]+)?");
        private readonly Regex _name = new(              @"(?:NA - )?(?:([\S\s]+) - )?([\S\s]+)? xd\.mp3");
        private readonly Regex   _id = new(@"(?:(?:\?v=)|(?:v\/)|(?:\.be\/)|(?:embed\/))([\w\d]+)");
        
        // input: /song URL [artist - ] [title] -qn
        // -q - mid quality
        // -n - name format: Title.mp3 (default: Artist - Title.mp3)
        // -p - extract preview from video
        // -c - remove text in (brackets)
        public override void Run()
        {
            if (Bot.ThorRagnarok.ChatIsBanned(Chat)) return;

            var args = _args.Match(Text);

            if (args.Success)
            {
                var url    = args.Groups[1].Value;
                var artist = args.Groups[2].Success ? args.Groups[2].Value : null;
                var title  = args.Groups[3].Success ? args.Groups[3].Value : null;

                var id = _id.Match(url).Groups[1].Value;

                DownloadSongAsync(id, artist, title, SnapshotMessageData());
            }
            else
            {
                Bot.SendMessage(Chat, "*SONG MANUAL*");
            }
        }

        private async void DownloadSongAsync(string url, string artist, string title, CommandParams cp)
        {
            var cmd_a = $"/C yt-dlp --no-mtime -f 251 -k -x --audio-format mp3 --audio-quality 0 {url} -o \"{artist ?? "%(artist)s"} - {title ?? "%(title)s"} xd\"";
            var cmd_v = $"/C yt-dlp --no-mtime -f \"bv*[height<=720][filesize<15M]\" -k {url} -o \"video xd.%(ext)s\"";

            var dir = $"{TEMP_FOLDER}/{DateTime.Now.Ticks}";
            Directory.CreateDirectory(dir);

            await DownloadShit(cmd_a, dir, cp.Chat);
            await DownloadShit(cmd_v, dir, cp.Chat);

            var di = new DirectoryInfo(dir);
            var mp4 = di.GetFiles("video xd.*")[0].FullName;
            var mp3 = di.GetFiles(   "*xd.mp3")[0].FullName;

            var xd = _name.Match(Path.GetFileName(mp3));
            if (artist is null && xd.Groups[1].Success) artist = xd.Groups[1].Value;
            if (title  is null && xd.Groups[2].Success) title  = xd.Groups[2].Value;

            var art = new F_Resize(mp4).ExportThumbnail();
            var track = new F_Overlay(mp3, art).AddTrackMetadata(artist, title);
            var jpg = new F_Resize(art).Transcode(".jpg");

            await using var stream = File.OpenRead(track);
            Bot.SendAudio(cp.Chat, new InputOnlineFile(stream, track), jpg);
            Log($"{cp.Title} >> SONG [mp3]");
        }

        private static async Task DownloadShit(string cmd, string dir, long chat)
        {
            var info = new ProcessStartInfo("cmd.exe", cmd) { WorkingDirectory = dir };
            var process = new Process() { StartInfo = info };
            process.Start();
            await RunSafelyAsync(process.WaitForExitAsync(), chat);
        }

        private static async Task RunSafelyAsync(Task task, long chat)
        {
            try
            {
                await task;
            }
            catch (Exception e)
            {
                LogError($"{chat} >> BRUH -> {FixedErrorMessage(e.Message)}");
            }
        }

        // yt-dlp --no-mtime -k -x --audio-format mp3 [--audio-quality 0] [--write-thumbnail] https://youtu.be/ZGSt0b0pAHw -o "%(artist)s - %(title)s xd"

        // yt-dlp --no-mtime -f 251 -k -x --audio-format mp3 [--audio-quality 0] [--write-thumbnail] https://youtu.be/ZGSt0b0pAHw -o "%(artist)s - %(title)s xd"
        // yt-dlp --no-mtime -f b -k https://youtu.be/ZGSt0b0pAHw -o "%(artist)s - %(title)s xd.%(ext)s"

        // ffmpeg -i "MONOGATARI Series - Hourousha xd.webm" -frames:v 1 -vf scale=-1:640 art3.png
        // ffmpeg -i "MONOGATARI Series - Hourousha xd.mp3" -i art3.png -map 0:0 -map 1:0 -c copy -id3v2_version 3 -metadata:s:v title="Album cover" -metadata:s:v comment="Cover (front)" -metadata artist="MONOGATARI OST" -metadata title="Hourousha" out_md.mp3
    }
}