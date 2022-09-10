using System;
using static Witlesss.Also.Strings;
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

            var failed = true;
            var start  = TimeSpan.Zero;
            var length = TimeSpan.Zero;

            var s = Text.Split();
            if (s.Length == 2 && StringIsTimeSpan(s[1], out length))
            {
                failed = false;
            }
            else if (s.Length > 2 && StringIsTimeSpan(s[1], out start))
            {
                if (!StringIsTimeSpan(s[2], out length))
                {
                    length = TimeSpan.Zero;
                }
                failed = false;
            }
            
            if (failed)
            {
                Bot.SendMessage(Chat, CUT_MANUAL);
                return;
            }
            
            Download(fileID, out string path, out var type);
            
            string result = Bot.MemeService.Cut(path, start, length);
            SendResult(result, type, VideoFilename, AudioFilename);
            Log($"{Title} >> CUT [8K]");

            string AudioFilename() => $"((({ValidFileName(SenderName(Message))}))).mp3";
            string VideoFilename() => "cut_fap_club.mp4";
        }
    }
}