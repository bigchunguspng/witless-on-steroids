using System;
using static System.TimeSpan;
using static Witlesss.Strings;
using static Witlesss.Extension;
using static Witlesss.Logger;

namespace Witlesss.Commands
{
    public class Cut : RemoveBitrate
    {
        public override void Run()
        {
            if (NothingToProcess()) return;

            var x = GetArgs();
            if (x.failed)
            {
                Bot.SendMessage(Chat, CUT_MANUAL);
                return;
            }
            
            Download(FileID, out string path, out var type);
            
            string result = Bot.MemeService.Cut(path, x.start, x.length);
            SendResult(result, type, VideoFilename, AudioFilename);
            Log($"{Title} >> CUT [8K-]");

            string AudioFilename() => MediaFileName($"((({Sender()}))).mp3");
            string VideoFilename() => "cut_fap_club.mp4";
        }

        protected (bool failed, TimeSpan start, TimeSpan length) GetArgs()
        {
            var s = Text.Split();
            int len = s.Length;
            if      (len == 2 && TextIsTimeSpan(s[1], out var length)) return (false, Zero, length);       // [++]----]
            else if (len >= 3 && TextIsTimeSpan(s[1], out var start))
            {
                if  (len == 4 && TextIsTimeSpan(s[3], out var end))    return (false, start, end - start); // [-[++]--]
                else if         (TextIsTimeSpan(s[2], out length))     return (false, start, length);      // [-[++]--]
                else                                                   return (false, start, Zero);        // [-[+++++]
            }
            else                                                       return (true, Zero, Zero);          // [-------]
        }
    }
}