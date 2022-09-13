using System;
using static Witlesss.Strings;
using static Witlesss.Extension;
using static Witlesss.Logger;

namespace Witlesss.Commands
{
    public class Cut : RemoveBitrate
    {
        public override void Run()
        {
            string fileID = GetVideoOrAudioID();
            if (fileID == null) return;

            var x = GetArgs();
            if (x.failed)
            {
                Bot.SendMessage(Chat, CUT_MANUAL);
                return;
            }
            
            Download(fileID, out string path, out var type);
            
            string result = Bot.MemeService.Cut(path, x.start, x.length);
            SendResult(result, type, VideoFilename, AudioFilename);
            Log($"{Title} >> CUT [8K-]");

            string AudioFilename() => MediaFileName($"((({Sender()}))).mp3");
            string VideoFilename() => "cut_fap_club.mp4";
        }

        protected (bool failed, TimeSpan start, TimeSpan length) GetArgs()
        {
            var s = Text.Split();
            if (s.Length == 2 && StringIsTimeSpan(s[1], out var length))
                return (false, TimeSpan.Zero, length);    // [++++]-----]
            else if (s.Length > 2 && StringIsTimeSpan(s[1], out var start))
            {
                if (StringIsTimeSpan(s[2], out length))
                    return (false, start, length);        // [--[++++]--]
                else
                    return (false, start, TimeSpan.Zero); // [--[+++++++]
            }
            
            return (true, TimeSpan.Zero, TimeSpan.Zero);  // [----------]
        }
    }
}