using System.Linq;
using System.Text.RegularExpressions;
using Telegram.Bot.Types.InputFiles;

namespace Witlesss.Commands.Editing
{
    public class Scale : VideoCommand
    {
        private const string _filename = "scale_fap_club.mp4";

        private readonly Regex _number = new(@"^\d+(\.\d+)*$");

        public override void Run()
        {
            if (NoVideo()) return;

            if (Text.Contains(' '))
            {
                var args = Text.Split(' ').Skip(1).Take(2).ToArray();

                var cmd = Text.ToLower();
                if (cmd.Contains("scales") || cmd.Contains("scalex"))
                {
                    if (args.Length > 0 && _number.IsMatch(args[0])) args[0] = args[0] + "*w";
                    if (args.Length > 1 && _number.IsMatch(args[1])) args[1] = args[1] + "*h";
                }

                for (var i = 0; i < args.Length; i++)
                {
                    var w = Regex.Replace(args[i], "(?<=[^io_]|^)w",           "iw");
                    var h = Regex.Replace(w,       "(?<=[^io_]|^)h(?=[^s]|$)", "ih");
                    args[i] = h;
                }

                if (args.Length == 1) args = new[] { args[0], "-1" };

                for (var i = 0; i < args.Length; i++) // fixing oddness and large size
                {
                    var w = i == 0;
                    if (args[i] == "-1") args[i] = w ? "iw*oh/ih" : "ih*ow/iw";
                    args[i] = $"min({(w ? "1920" : "1080")},ceil(({args[i]})/2)*2)";
                }

                Bot.Download(FileID, Chat, out var path, out var type);

                SendResult(Memes.Scale(path, args), type, _filename);
                Log($"{Title} >> SCALE [{string.Join(':', args)}]");
            }
            else
                Bot.SendMessage(Chat, SCALE_MANUAL);
        }

        protected static void SendResult(string result, MediaType type, string filename)
        {
            using var stream = File.OpenRead(result);
            if      (type == MediaType.Video) Bot.SendAnimation(Chat, new InputOnlineFile(stream, filename));
            else if (type == MediaType.Movie) Bot.SendVideo    (Chat, new InputOnlineFile(stream, filename));
            else if (type == MediaType.Round) Bot.SendVideoNote(Chat, new InputOnlineFile(stream));
        }
    }
}