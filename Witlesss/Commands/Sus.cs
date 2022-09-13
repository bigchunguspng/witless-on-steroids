using System;
using static Witlesss.Strings;
using static Witlesss.Logger;

namespace Witlesss.Commands
{
    public class Sus : Cut
    {
        public override void Run()
        {
            string fileID = GetVideoOrAudioID();
            if (fileID == null) return;

            var argless = false;
            var x = GetArgs();
            if (x.failed)
            {
                if (Text.Contains(' '))
                {
                    Bot.SendMessage(Chat, SUS_MANUAL);
                    return;
                }
                else argless = true;
            }

            Download(fileID, out string path, out var type);

            if (argless) x.length = TimeSpan.MinValue;

            string result = Bot.MemeService.Sus(path, x.start, x.length, type);
            SendResult(result, type, VideoFilename, AudioFilename);
            Log($"{Title} >> SUS [>_<]");

            string AudioFilename() => MediaFileName($"Kid Named {WhenTheSenderIsSus()}.mp3");
            string VideoFilename() => "sus_fap_club.mp4";

            string WhenTheSenderIsSus()
            {
                string s = Sender();
                return s.Length > 2 ? s[..2] + s[0] : s;
            }
        }
    }
}