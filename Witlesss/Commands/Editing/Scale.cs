using System.Linq;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;

namespace Witlesss.Commands.Editing
{
    public class Scale : VideoCommand
    {
        private readonly Regex _number = new(@"^\d+(\.\d+)?$");

        public override void Run()
        {
            if (NothingToProcess()) return;

            if (Text.Contains(' '))
            {
                var args = Text.Split(' ').Skip(1).Take(2).ToArray();

                MultiplyIfArgIsNumber(0, 'w');
                MultiplyIfArgIsNumber(1, 'h');

                void MultiplyIfArgIsNumber(int i, char side)
                {
                    if (args.Length > i && _number.IsMatch(args[i]))
                    {
                        var d = double.TryParse(args[i].Replace('.', ','), out var value);
                        if (d && value < 5) args[i] = args[i] + '*' + side;
                    }
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

                SendResult(Memes.Scale(path, args), type);
                Log($"{Title} >> SCALE [{string.Join(':', args)}]");
            }
            else
                Bot.SendMessage(Chat, SCALE_MANUAL);
        }

        protected override string VideoFileName { get; } = "scale_fap_club.mp4";

        protected override bool MessageContainsFile(Message m)
        {
            return GetVideoFileID(m) || GetPhotoFileID(m);
        }
    }
}