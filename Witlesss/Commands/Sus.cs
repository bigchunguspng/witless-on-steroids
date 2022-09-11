using System;
using System.Linq;
using static Witlesss.Strings;
using static Witlesss.Extension;
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

            if (argless) x.length = TimeSpan.FromSeconds(Bot.MemeService.GetDurationInSeconds(path) / 2D);

            string result = Bot.MemeService.Sus(path, x.start, x.length, type);
            SendResult(result, type, VideoFilename, AudioFilename);
            Log($"{Title} >> SUS [>><<]");

            string AudioFilename() => $"Kid Named {WhenTheSenderIsSus()}.mp3";
            string VideoFilename() => "sus_fap_club.mp4";

            string WhenTheSenderIsSus()
            {
                string s = ValidFileName(SenderName(Message));
                int l = s.Length / 2;
                return s.Remove(l) + new string(s.Remove(l - 1).Reverse().ToArray());
            }
        }
    }
}