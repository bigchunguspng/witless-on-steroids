using System;
using System.Linq;
using System.Text.RegularExpressions;
using Telegram.Bot.Types.InputFiles;

namespace Witlesss.Commands.Editing
{
    public class Crop : VideoCommand
    {
        private const string _filename = "piece_fap_crop.mp4";

        public override void Run()
        {
            if (NoVideo()) return;

            if (Text.Contains(' '))
            {
                var args = Text.Split(' ').Skip(1).ToArray();

                if (args[0].StartsWith("shake"))
                {
                    var crop   = 1 < args.Length ? args[1] : "0.95";
                    var speed  = 2 < args.Length ? args[2] : "random(0)";
                    var offset = 3 < args.Length ? args[3] : "random(0)";
                    args = F_Shake(crop, speed, offset).Split();
                }
                
                for (var i = 0; i < Math.Min(args.Length, 4); i++)
                {
                    var w   = Regex.Replace(args[i], "(?<=[^io_]|^)w",           "iw");
                    args[i] = Regex.Replace(w,       "(?<=[^io_]|^)h(?=[^s]|$)", "ih");
                }

                if (args.Length > 4) args = args.Take(4).ToArray();

                Bot.Download(FileID, Chat, out var path, out var type);
                
                SendResult(Memes.Crop(path, args), type);
                Log($"{Title} >> CROP [{string.Join(':', args)}]");
            }
            else
                Bot.SendMessage(Chat, CROP_MANUAL);
        }

        private static void SendResult(string result, MediaType type)
        {
            using var stream = File.OpenRead(result);
            if      (type == MediaType.Video) Bot.SendAnimation(Chat, new InputOnlineFile(stream, _filename));
            else if (type == MediaType.Movie) Bot.SendVideo    (Chat, new InputOnlineFile(stream, _filename));
            else if (type == MediaType.Round) Bot.SendVideoNote(Chat, new InputOnlineFile(stream));
        }

        private static string F_Shake(string a, string b, string c)
        {
            return $"w*{a} h*{a} (w-out_w)/2+((w-out_w)/2)*sin(t*{b}-{c}) (h-out_h)/2+((h-out_h)/2)*sin(t*{b}+2*{c})";
        }
    }
}