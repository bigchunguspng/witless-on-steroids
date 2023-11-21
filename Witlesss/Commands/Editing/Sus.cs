using System;

namespace Witlesss.Commands.Editing
{
    public class Sus : Cut
    {
        public override void Run()
        {
            if (NothingToProcess()) return;

            var argless = false;
            var x = GetArgs();
            if (x.failed)
            {
                if (Text.Contains(' '))
                {
                    Bot.SendMessage(Chat, SUS_MANUAL);
                    return;
                }
                argless = true;
            }

            Bot.Download(FileID, Chat, out string path, out var type);

            if (argless) x.length = TimeSpan.MinValue;

            string result = Memes.Sus(path, new CutSpan(x.start, x.length));
            SendResult(result, type, VideoFilename, AudioFilename);
            Log($"{Title} >> SUS [>_<]");

            string AudioFilename() => SongNameOr($"Kid Named {WhenTheSenderIsSus()}.mp3");
            string VideoFilename() => "sus_fap_club.mp4";

            string WhenTheSenderIsSus() => Sender.Length > 2 ? Sender[..2] + Sender[0] : Sender;
        }
    }
}