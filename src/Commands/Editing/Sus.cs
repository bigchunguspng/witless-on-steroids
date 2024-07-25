using System;
using System.Threading.Tasks;
using Witlesss.MediaTools;

namespace Witlesss.Commands.Editing
{
    public class Sus : VideoCommand
    {
        protected override async Task Execute()
        {
            var argless = false;
            var x = Cut.ParseArgs(Args?.Split());
            if (x.failed)
            {
                if (Args is not null)
                {
                    Bot.SendMessage(Chat, SUS_MANUAL);
                    return;
                }
                argless = true;
            }

            var (path, type) = await Bot.Download(FileID, Chat);

            if (argless) x.length = TimeSpan.MinValue;

            var result = await FFMpegXD.Sus(path, new CutSpan(x.start, x.length));
            SendResult(result, type);
            Log($"{Title} >> SUS [>_<]");
        }

        protected override string AudioFileName => SongNameOr($"Kid Named {WhenTheSenderIsSus()}.mp3");
        protected override string VideoFileName { get; } = "sus_fap_club.mp4";

        private string WhenTheSenderIsSus() => Sender.Length > 2 ? Sender[..2] + Sender[0] : Sender;
    }
}