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
                var args = Text.Split(' ', 5).Skip(1).ToArray();
                for (var i = 0; i < args.Length; i++)
                {
                    var w   = Regex.Replace(args[i], "(?<=[^io_]|^)w",         "iw");
                    args[i] = Regex.Replace(w,       "(?<=[^io_]|^)h(?=[^s]|$)", "ih");
                }

                Bot.Download(FileID, Chat, out var path, out var type);
                
                SendResult(Memes.Crop(path, args), type);
                Log($"{Title} >> CROP [{string.Join(':', args)}]");
            }
            else
            {
                Bot.SendMessage(Chat, CUT_MANUAL); //todo manual
            }
        }

        private static void SendResult(string result, MediaType type)
        {
            using var stream = File.OpenRead(result);
            if      (type == MediaType.Video) Bot.SendAnimation(Chat, new InputOnlineFile(stream, _filename));
            else if (type == MediaType.Movie) Bot.SendVideo    (Chat, new InputOnlineFile(stream, _filename));
            else if (type == MediaType.Round) Bot.SendVideoNote(Chat, new InputOnlineFile(stream));
        }
    }
}

// (\d+|[\+\-*\/\(\)]|(in_w|iw|out_w|ow|in_h|ih|out_h|oh)|[xynt]|(sin|cos|PHI|pos|[sd]ar|a|[hv]sub))+

//  w/2 h/2 (w-out_w)/2+((w-out_w)/2)*sin(t*10) (h-out_h)/2+((h-out_h)/2)*sin(t*13)

// /crop w/2 h/2 (w-out_w)/2+((w-out_w)/2)*sin(n/10) (h-out_h)/2+((h-out_h)/2)*sin(n/7)

// /crop w*0.75 h*0.75 (w-out_w)/2+((w-out_w)*2/3)*sin(t*60) (h-out_h)*2/3+((h-out_h)/2)*sin(t*60) - s_shake